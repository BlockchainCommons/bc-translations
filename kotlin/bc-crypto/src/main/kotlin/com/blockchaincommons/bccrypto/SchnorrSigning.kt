package com.blockchaincommons.bccrypto

import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator
import org.bouncycastle.math.ec.ECPoint
import org.bouncycastle.math.ec.custom.sec.SecP256K1Curve
import java.math.BigInteger
import java.security.MessageDigest

const val SCHNORR_SIGNATURE_SIZE = 64

private val curve = SecP256K1Curve()
private val secp256k1N: BigInteger = curve.order
private val secp256k1P: BigInteger = curve.field.characteristic
private val secp256k1G: ECPoint = curve.createPoint(
    BigInteger("79BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798", 16),
    BigInteger("483ADA7726A3C4655DA4FBFC0E1108A8FD17B448A68554199C47D08FFB10D4B8", 16),
)

private fun taggedHash(tag: String, vararg data: ByteArray): ByteArray {
    val tagHash = MessageDigest.getInstance("SHA-256").digest(tag.toByteArray())
    val digest = MessageDigest.getInstance("SHA-256")
    digest.update(tagHash)
    digest.update(tagHash)
    for (d in data) digest.update(d)
    return digest.digest()
}

private fun bytesFromBigInteger(n: BigInteger): ByteArray {
    val b = n.toByteArray()
    return when {
        b.size == 32 -> b
        b.size > 32 -> b.copyOfRange(b.size - 32, b.size)
        else -> ByteArray(32 - b.size) + b
    }
}

private fun bigIntegerFromBytes(b: ByteArray): BigInteger =
    BigInteger(1, b)

private fun hasEvenY(point: ECPoint): Boolean {
    val normalized = point.normalize()
    return normalized.affineYCoord.toBigInteger().mod(BigInteger.TWO) == BigInteger.ZERO
}

private fun xBytes(point: ECPoint): ByteArray {
    val normalized = point.normalize()
    return bytesFromBigInteger(normalized.affineXCoord.toBigInteger())
}

private fun liftX(xBytes: ByteArray): ECPoint {
    val x = bigIntegerFromBytes(xBytes)
    require(x < secp256k1P) { "Public key x-coordinate exceeds field size" }
    val c = x.modPow(BigInteger.valueOf(3), secp256k1P).add(BigInteger.valueOf(7)).mod(secp256k1P)
    val exp = secp256k1P.add(BigInteger.ONE).divide(BigInteger.valueOf(4))
    val y = c.modPow(exp, secp256k1P)
    require(y.modPow(BigInteger.TWO, secp256k1P) == c) { "Public key not on curve" }
    val yFinal = if (y.mod(BigInteger.TWO) == BigInteger.ZERO) y else secp256k1P.subtract(y)
    return curve.createPoint(x, yFinal)
}

fun schnorrSign(
    ecdsaPrivateKey: ByteArray,
    message: ByteArray,
): ByteArray {
    val rng = SecureRandomNumberGenerator()
    return schnorrSignUsing(ecdsaPrivateKey, message, rng)
}

fun schnorrSignUsing(
    ecdsaPrivateKey: ByteArray,
    message: ByteArray,
    rng: RandomNumberGenerator,
): ByteArray {
    val auxRand = rng.randomData(32)
    return schnorrSignWithAuxRand(ecdsaPrivateKey, message, auxRand)
}

fun schnorrSignWithAuxRand(
    ecdsaPrivateKey: ByteArray,
    message: ByteArray,
    auxRand: ByteArray,
): ByteArray {
    val dPrime = bigIntegerFromBytes(ecdsaPrivateKey)
    require(dPrime != BigInteger.ZERO && dPrime < secp256k1N) { "Invalid private key" }

    val p = secp256k1G.multiply(dPrime)
    val d = if (hasEvenY(p)) dPrime else secp256k1N.subtract(dPrime)
    val pBytes = xBytes(p)

    val t = ByteArray(32)
    val dBytes = bytesFromBigInteger(d)
    val auxHash = taggedHash("BIP0340/aux", auxRand)
    for (i in 0 until 32) t[i] = (dBytes[i].toInt() xor auxHash[i].toInt()).toByte()

    val rand = taggedHash("BIP0340/nonce", t, pBytes, message)
    val kPrime = bigIntegerFromBytes(rand).mod(secp256k1N)
    require(kPrime != BigInteger.ZERO) { "Failure: k' is zero" }

    val r = secp256k1G.multiply(kPrime)
    val k = if (hasEvenY(r)) kPrime else secp256k1N.subtract(kPrime)
    val rBytes = xBytes(r)

    val e = bigIntegerFromBytes(taggedHash("BIP0340/challenge", rBytes, pBytes, message))
        .mod(secp256k1N)
    val sig = rBytes + bytesFromBigInteger(k.add(e.multiply(d)).mod(secp256k1N))
    return sig
}

fun schnorrVerify(
    schnorrPublicKey: ByteArray,
    schnorrSignature: ByteArray,
    message: ByteArray,
): Boolean {
    require(schnorrPublicKey.size == 32)
    require(schnorrSignature.size == 64)

    val p = liftX(schnorrPublicKey)
    val r = bigIntegerFromBytes(schnorrSignature.copyOfRange(0, 32))
    val s = bigIntegerFromBytes(schnorrSignature.copyOfRange(32, 64))

    if (r >= secp256k1P || s >= secp256k1N) return false

    val e = bigIntegerFromBytes(
        taggedHash(
            "BIP0340/challenge",
            schnorrSignature.copyOfRange(0, 32),
            xBytes(p),
            message,
        ),
    ).mod(secp256k1N)

    val rPoint = secp256k1G.multiply(s).add(p.negate().multiply(e))
    if (rPoint.isInfinity) return false
    if (!hasEvenY(rPoint)) return false
    val rPointX = rPoint.normalize().affineXCoord.toBigInteger()
    return rPointX == r
}
