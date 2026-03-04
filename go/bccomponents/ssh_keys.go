package bccomponents

import (
	"crypto"
	"crypto/ecdsa"
	"crypto/ed25519"
	"crypto/elliptic"
	"crypto/rand"
	"crypto/sha256"
	"crypto/sha512"
	"encoding/binary"
	"encoding/pem"
	"fmt"
	"hash"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	"golang.org/x/crypto/ssh"
)

// SSHPrivateKey wraps an SSH private key.
type SSHPrivateKey struct {
	signer  ssh.Signer
	rawPEM  []byte
	comment string
}

// SSHPrivateKeyFromOpenSSH parses an OpenSSH-format private key.
func SSHPrivateKeyFromOpenSSH(pemData []byte) (*SSHPrivateKey, error) {
	signer, err := ssh.ParsePrivateKey(pemData)
	if err != nil {
		return nil, errSSH(err.Error())
	}
	return &SSHPrivateKey{signer: signer, rawPEM: pemData}, nil
}

// SSHPrivateKeyFromSigner wraps a crypto.Signer as an SSH private key.
func SSHPrivateKeyFromSigner(key crypto.Signer, comment string) (*SSHPrivateKey, error) {
	signer, err := ssh.NewSignerFromKey(key)
	if err != nil {
		return nil, errSSH(err.Error())
	}
	return &SSHPrivateKey{signer: signer, comment: comment}, nil
}

// PublicKey returns the corresponding SSH public key.
func (k *SSHPrivateKey) PublicKey() *SSHPublicKey {
	return &SSHPublicKey{key: k.signer.PublicKey()}
}

// Comment returns the key comment.
func (k *SSHPrivateKey) Comment() string { return k.comment }

// OpenSSHBytes returns the key in OpenSSH PEM format.
func (k *SSHPrivateKey) OpenSSHBytes() []byte {
	if k.rawPEM != nil {
		return k.rawPEM
	}
	return nil
}

// String returns a human-readable representation.
func (k *SSHPrivateKey) String() string {
	return fmt.Sprintf("SSHPrivateKey(%s)", k.signer.PublicKey().Type())
}

// SignSSH signs a message using the SSH signature format.
func (k *SSHPrivateKey) SignSSH(message []byte, namespace string, hashAlg SSHHashAlg) (*SSHSignature, error) {
	var h hash.Hash
	var algName string
	switch hashAlg {
	case SSHHashSHA256:
		h = sha256.New()
		algName = "sha256"
	case SSHHashSHA512:
		h = sha512.New()
		algName = "sha512"
	default:
		return nil, errSSH("unsupported hash algorithm")
	}

	h.Write(message)
	msgHash := h.Sum(nil)

	signedData := buildSSHSigSignedData(namespace, algName, msgHash)

	sig, err := k.signer.Sign(rand.Reader, signedData)
	if err != nil {
		return nil, errSSH(err.Error())
	}

	return &SSHSignature{
		publicKey: k.signer.PublicKey(),
		namespace: namespace,
		hashAlg:   algName,
		signature: sig,
	}, nil
}

// buildSSHSigSignedData creates the signed data for SSH signature format.
func buildSSHSigSignedData(namespace, hashAlg string, msgHash []byte) []byte {
	magic := []byte("SSHSIG")
	var buf []byte
	buf = append(buf, magic...)
	buf = appendSSHString(buf, namespace)
	buf = appendSSHString(buf, "")
	buf = appendSSHString(buf, hashAlg)
	buf = appendSSHBytes(buf, msgHash)
	return buf
}

func appendSSHString(buf []byte, s string) []byte {
	return appendSSHBytes(buf, []byte(s))
}

func appendSSHBytes(buf, data []byte) []byte {
	var lenBuf [4]byte
	binary.BigEndian.PutUint32(lenBuf[:], uint32(len(data)))
	buf = append(buf, lenBuf[:]...)
	buf = append(buf, data...)
	return buf
}

// --- SSHPublicKey ---

// SSHPublicKey wraps an SSH public key.
type SSHPublicKey struct {
	key ssh.PublicKey
}

// SSHPublicKeyFromAuthorizedKey parses an SSH public key from authorized_keys format.
func SSHPublicKeyFromAuthorizedKey(data []byte) (*SSHPublicKey, error) {
	key, _, _, _, err := ssh.ParseAuthorizedKey(data)
	if err != nil {
		return nil, errSSH(err.Error())
	}
	return &SSHPublicKey{key: key}, nil
}

// SSHPublicKeyFromKey wraps an ssh.PublicKey.
func SSHPublicKeyFromKey(key ssh.PublicKey) *SSHPublicKey {
	return &SSHPublicKey{key: key}
}

// AuthorizedKeyBytes returns the key in authorized_keys format.
func (k *SSHPublicKey) AuthorizedKeyBytes() []byte {
	return ssh.MarshalAuthorizedKey(k.key)
}

// Type returns the key type string.
func (k *SSHPublicKey) Type() string { return k.key.Type() }

// Verify verifies an SSH signature over a message.
func (k *SSHPublicKey) Verify(sig *SSHSignature, message []byte) bool {
	var h hash.Hash
	switch sig.hashAlg {
	case "sha256":
		h = sha256.New()
	case "sha512":
		h = sha512.New()
	default:
		return false
	}
	h.Write(message)
	msgHash := h.Sum(nil)

	signedData := buildSSHSigSignedData(sig.namespace, sig.hashAlg, msgHash)
	return k.key.Verify(signedData, sig.signature) == nil
}

// String returns a human-readable representation.
func (k *SSHPublicKey) String() string {
	return fmt.Sprintf("SSHPublicKey(%s)", k.key.Type())
}

// --- SSHSignature ---

// SSHSignature represents an SSH signature in the SSHSIG format.
type SSHSignature struct {
	publicKey ssh.PublicKey
	namespace string
	hashAlg   string
	signature *ssh.Signature
}

// Scheme returns the signature scheme based on the key type.
func (s *SSHSignature) Scheme() SignatureScheme {
	switch s.publicKey.Type() {
	case "ssh-ed25519":
		return SchemeSSHEd25519
	case "ssh-dss":
		return SchemeSSHDSA
	case "ecdsa-sha2-nistp256":
		return SchemeSSHEcdsaP256
	case "ecdsa-sha2-nistp384":
		return SchemeSSHEcdsaP384
	default:
		return SchemeSSHEd25519
	}
}

// PEM returns the signature in SSH signature PEM format.
func (s *SSHSignature) PEM() []byte {
	var buf []byte
	buf = append(buf, []byte("SSHSIG")...)

	var version [4]byte
	binary.BigEndian.PutUint32(version[:], 1)
	buf = append(buf, version[:]...)

	pubBytes := s.publicKey.Marshal()
	buf = appendSSHBytes(buf, pubBytes)
	buf = appendSSHString(buf, s.namespace)
	buf = appendSSHString(buf, "")
	buf = appendSSHString(buf, s.hashAlg)

	sigBytes := ssh.Marshal(s.signature)
	buf = appendSSHBytes(buf, sigBytes)

	return pem.EncodeToMemory(&pem.Block{
		Type:  "SSH SIGNATURE",
		Bytes: buf,
	})
}

// SSHSignatureFromPEM parses an SSH signature from PEM format.
func SSHSignatureFromPEM(pemData []byte) (*SSHSignature, error) {
	block, _ := pem.Decode(pemData)
	if block == nil || block.Type != "SSH SIGNATURE" {
		return nil, errSSH("invalid SSH signature PEM")
	}

	data := block.Bytes
	if len(data) < 6 || string(data[:6]) != "SSHSIG" {
		return nil, errSSH("invalid SSH signature magic")
	}
	data = data[6:]

	if len(data) < 4 {
		return nil, errSSH("truncated version")
	}
	data = data[4:]

	pubKeyBytes, rest, err := parseSSHLenBytes(data)
	if err != nil {
		return nil, errSSH("bad public key")
	}
	data = rest

	pubKey, err := ssh.ParsePublicKey(pubKeyBytes)
	if err != nil {
		return nil, errSSH(err.Error())
	}

	nsBytes, rest, err := parseSSHLenBytes(data)
	if err != nil {
		return nil, errSSH("bad namespace")
	}
	data = rest

	_, rest, err = parseSSHLenBytes(data)
	if err != nil {
		return nil, errSSH("bad reserved")
	}
	data = rest

	hashAlgBytes, rest, err := parseSSHLenBytes(data)
	if err != nil {
		return nil, errSSH("bad hash algorithm")
	}
	data = rest

	sigBytes, _, err := parseSSHLenBytes(data)
	if err != nil {
		return nil, errSSH("bad signature")
	}

	sig := new(ssh.Signature)
	if err := ssh.Unmarshal(sigBytes, sig); err != nil {
		return nil, errSSH(err.Error())
	}

	return &SSHSignature{
		publicKey: pubKey,
		namespace: string(nsBytes),
		hashAlg:   string(hashAlgBytes),
		signature: sig,
	}, nil
}

func parseSSHLenBytes(data []byte) ([]byte, []byte, error) {
	if len(data) < 4 {
		return nil, nil, fmt.Errorf("truncated")
	}
	length := binary.BigEndian.Uint32(data[:4])
	data = data[4:]
	if uint32(len(data)) < length {
		return nil, nil, fmt.Errorf("truncated")
	}
	return data[:length], data[length:], nil
}

// DecodeSSHSignature decodes an SSH signature from tagged CBOR.
func DecodeSSHSignature(cbor dcbor.CBOR) (*SSHSignature, error) {
	_, inner, ok := cbor.AsTaggedValue()
	if !ok {
		return nil, dcbor.NewErrorf("expected tagged CBOR for SSH signature")
	}
	text, ok := inner.AsText()
	if !ok {
		return nil, dcbor.NewErrorf("SSH signature must be text")
	}
	return SSHSignatureFromPEM([]byte(text))
}

// ToCBOR encodes the SSH signature as tagged CBOR text.
func (s *SSHSignature) ToCBOR() dcbor.CBOR {
	return dcbor.NewCBORTagged(dcbor.TagWithValue(bctags.TagSSHTextSignature), dcbor.MustFromAny(string(s.PEM())))
}

// --- SSH key generation helpers ---

// generateSSHEd25519Key generates a deterministic SSH Ed25519 keypair.
func generateSSHEd25519Key(seed []byte, comment string) (*SSHPrivateKey, error) {
	privateKey := ed25519.NewKeyFromSeed(seed)
	return SSHPrivateKeyFromSigner(privateKey, comment)
}

// generateSSHECDSAKey generates an ECDSA SSH keypair for the given curve.
func generateSSHECDSAKey(curve elliptic.Curve, comment string) (*SSHPrivateKey, error) {
	key, err := ecdsa.GenerateKey(curve, rand.Reader)
	if err != nil {
		return nil, errSSH(err.Error())
	}
	return SSHPrivateKeyFromSigner(key, comment)
}
