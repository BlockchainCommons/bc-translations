package com.blockchaincommons.bclifehash

import java.awt.image.BufferedImage
import java.io.File
import javax.imageio.ImageIO
import kotlin.test.Test

class GeneratePngsTest {
    @Test
    fun generatePngs() {
        val versions = listOf(
            "version1" to Version.Version1,
            "version2" to Version.Version2,
            "detailed" to Version.Detailed,
            "fiducial" to Version.Fiducial,
            "grayscale_fiducial" to Version.GrayscaleFiducial,
        )

        val outDir = File("out")

        for ((name, version) in versions) {
            val dir = File(outDir, name)
            dir.mkdirs()

            for (i in 0 until 100) {
                val input = i.toString()
                val image = LifeHash.fromUtf8(input, version, 1, false)

                val buffered = BufferedImage(image.width, image.height, BufferedImage.TYPE_INT_RGB)
                for (y in 0 until image.height) {
                    for (x in 0 until image.width) {
                        val offset = (y * image.width + x) * 3
                        val r = image.colors[offset].toUByte().toInt()
                        val g = image.colors[offset + 1].toUByte().toInt()
                        val b = image.colors[offset + 2].toUByte().toInt()
                        val rgb = (r shl 16) or (g shl 8) or b
                        buffered.setRGB(x, y, rgb)
                    }
                }

                ImageIO.write(buffered, "png", File(dir, "$i.png"))
            }
        }
    }
}
