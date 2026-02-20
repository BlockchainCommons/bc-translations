import BCRand
import Foundation
import XCTest
@testable import BCCrypto

final class SchnorrSigningTests: XCTestCase {
    private struct VectorCase {
        let secretKey: String?
        let publicKey: String
        let auxRand: String?
        let message: String
        let signature: String
        let verifies: Bool
    }

    private func runTestVector(_ vector: VectorCase) {
        if let secretKeyHex = vector.secretKey,
           let auxRandHex = vector.auxRand
        {
            let secretKey = Data(testHex: secretKeyHex)
            let auxRand = Data(testHex: auxRandHex)
            let message = Data(testHex: vector.message)
            let expectedPublicKey = Data(testHex: vector.publicKey)
            let expectedSignature = Data(testHex: vector.signature)

            XCTAssertEqual(
                schnorrPublicKeyFromPrivateKey(secretKey),
                expectedPublicKey
            )
            XCTAssertEqual(
                schnorrSignWithAuxRand(secretKey, message, auxRand),
                expectedSignature
            )
        }

        let verified = schnorrVerify(
            Data(testHex: vector.publicKey),
            Data(testHex: vector.signature),
            Data(testHex: vector.message)
        )
        XCTAssertEqual(verified, vector.verifies)
    }

    func testSchnorrSign() {
        var rng = makeFakeRandomNumberGenerator()
        let privateKey = ecdsaNewPrivateKeyUsing(&rng)
        XCTAssertEqual(
            privateKey,
            Data(
                testHex: "7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed"
            )
        )

        let message = Data("Hello World".utf8)
        let signature = schnorrSignUsing(privateKey, message, &rng)

        XCTAssertEqual(signature.count, 64)
        XCTAssertEqual(
            signature,
            Data(
                testHex: "8f6ec4edbe1a6d96edfc5f15e18e06a6e2559a3426c52d2c38fec17fe7e0cafc95177206d018662a279f2b571224cf07006939fc25d0cae7a7e7b44a4b25f543"
            )
        )

        let schnorrPublicKey = schnorrPublicKeyFromPrivateKey(privateKey)
        XCTAssertTrue(schnorrVerify(schnorrPublicKey, signature, message))
    }

    func testBIP340Vectors() {
        let vectors: [VectorCase] = [
            VectorCase(
                secretKey: "0000000000000000000000000000000000000000000000000000000000000003",
                publicKey: "F9308A019258C31049344F85F89D5229B531C845836F99B08601F113BCE036F9",
                auxRand: "0000000000000000000000000000000000000000000000000000000000000000",
                message: "0000000000000000000000000000000000000000000000000000000000000000",
                signature: "E907831F80848D1069A5371B402410364BDF1C5F8307B0084C55F1CE2DCA821525F66A4A85EA8B71E482A74F382D2CE5EBEEE8FDB2172F477DF4900D310536C0",
                verifies: true
            ),
            VectorCase(
                secretKey: "B7E151628AED2A6ABF7158809CF4F3C762E7160F38B4DA56A784D9045190CFEF",
                publicKey: "DFF1D77F2A671C5F36183726DB2341BE58FEAE1DA2DECED843240F7B502BA659",
                auxRand: "0000000000000000000000000000000000000000000000000000000000000001",
                message: "243F6A8885A308D313198A2E03707344A4093822299F31D0082EFA98EC4E6C89",
                signature: "6896BD60EEAE296DB48A229FF71DFE071BDE413E6D43F917DC8DCF8C78DE33418906D11AC976ABCCB20B091292BFF4EA897EFCB639EA871CFA95F6DE339E4B0A",
                verifies: true
            ),
            VectorCase(
                secretKey: "C90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B14E5C9",
                publicKey: "DD308AFEC5777E13121FA72B9CC1B7CC0139715309B086C960E18FD969774EB8",
                auxRand: "C87AA53824B4D7AE2EB035A2B5BBBCCC080E76CDC6D1692C4B0B62D798E6D906",
                message: "7E2D58D8B3BCDF1ABADEC7829054F90DDA9805AAB56C77333024B9D0A508B75C",
                signature: "5831AAEED7B44BB74E5EAB94BA9D4294C49BCF2A60728D8B4C200F50DD313C1BAB745879A5AD954A72C45A91C3A51D3C7ADEA98D82F8481E0E1E03674A6F3FB7",
                verifies: true
            ),
            VectorCase(
                secretKey: "0B432B2677937381AEF05BB02A66ECD012773062CF3FA2549E44F58ED2401710",
                publicKey: "25D1DFF95105F5253C4022F628A996AD3A0D95FBF21D468A1B33F8C160D8F517",
                auxRand: "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
                message: "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
                signature: "7EB0509757E246F19449885651611CB965ECC1A187DD51B64FDA1EDC9637D5EC97582B9CB13DB3933705B32BA982AF5AF25FD78881EBB32771FC5922EFC66EA3",
                verifies: true
            ),
            VectorCase(
                secretKey: nil,
                publicKey: "D69C3509BB99E412E68B0FE8544E72837DFA30746D8BE2AA65975F29D22DC7B9",
                auxRand: nil,
                message: "4DF3C3F68FCC83B27E9D42C90431A72499F17875C81A599B566C9889B9696703",
                signature: "00000000000000000000003B78CE563F89A0ED9414F5AA28AD0D96D6795F9C6376AFB1548AF603B3EB45C9F8207DEE1060CB71C04E80F593060B07D28308D7F4",
                verifies: true
            ),
            VectorCase(
                secretKey: nil,
                publicKey: "EEFDEA4CDB677750A420FEE807EACF21EB9898AE79B9768766E4FAA04A2D4A34",
                auxRand: nil,
                message: "243F6A8885A308D313198A2E03707344A4093822299F31D0082EFA98EC4E6C89",
                signature: "6CFF5C3BA86C69EA4B7376F31A9BCB4F74C1976089B2D9963DA2E5543E17776969E89B4C5564D00349106B8497785DD7D1D713A8AE82B32FA79D5F7FC407D39B",
                verifies: false
            ),
            VectorCase(
                secretKey: nil,
                publicKey: "DFF1D77F2A671C5F36183726DB2341BE58FEAE1DA2DECED843240F7B502BA659",
                auxRand: nil,
                message: "243F6A8885A308D313198A2E03707344A4093822299F31D0082EFA98EC4E6C89",
                signature: "FFF97BD5755EEEA420453A14355235D382F6472F8568A18B2F057A14602975563CC27944640AC607CD107AE10923D9EF7A73C643E166BE5EBEAFA34B1AC553E2",
                verifies: false
            ),
            VectorCase(
                secretKey: nil,
                publicKey: "DFF1D77F2A671C5F36183726DB2341BE58FEAE1DA2DECED843240F7B502BA659",
                auxRand: nil,
                message: "243F6A8885A308D313198A2E03707344A4093822299F31D0082EFA98EC4E6C89",
                signature: "1FA62E331EDBC21C394792D2AB1100A7B432B013DF3F6FF4F99FCB33E0E1515F28890B3EDB6E7189B630448B515CE4F8622A954CFE545735AAEA5134FCCDB2BD",
                verifies: false
            ),
            VectorCase(
                secretKey: nil,
                publicKey: "DFF1D77F2A671C5F36183726DB2341BE58FEAE1DA2DECED843240F7B502BA659",
                auxRand: nil,
                message: "243F6A8885A308D313198A2E03707344A4093822299F31D0082EFA98EC4E6C89",
                signature: "6CFF5C3BA86C69EA4B7376F31A9BCB4F74C1976089B2D9963DA2E5543E177769961764B3AA9B2FFCB6EF947B6887A226E8D7C93E00C5ED0C1834FF0D0C2E6DA6",
                verifies: false
            ),
            VectorCase(
                secretKey: nil,
                publicKey: "DFF1D77F2A671C5F36183726DB2341BE58FEAE1DA2DECED843240F7B502BA659",
                auxRand: nil,
                message: "243F6A8885A308D313198A2E03707344A4093822299F31D0082EFA98EC4E6C89",
                signature: "0000000000000000000000000000000000000000000000000000000000000000123DDA8328AF9C23A94C1FEECFD123BA4FB73476F0D594DCB65C6425BD186051",
                verifies: false
            ),
            VectorCase(
                secretKey: nil,
                publicKey: "DFF1D77F2A671C5F36183726DB2341BE58FEAE1DA2DECED843240F7B502BA659",
                auxRand: nil,
                message: "243F6A8885A308D313198A2E03707344A4093822299F31D0082EFA98EC4E6C89",
                signature: "00000000000000000000000000000000000000000000000000000000000000017615FBAF5AE28864013C099742DEADB4DBA87F11AC6754F93780D5A1837CF197",
                verifies: false
            ),
            VectorCase(
                secretKey: nil,
                publicKey: "DFF1D77F2A671C5F36183726DB2341BE58FEAE1DA2DECED843240F7B502BA659",
                auxRand: nil,
                message: "243F6A8885A308D313198A2E03707344A4093822299F31D0082EFA98EC4E6C89",
                signature: "4A298DACAE57395A15D0795DDBFD1DCB564DA82B0F269BC70A74F8220429BA1D69E89B4C5564D00349106B8497785DD7D1D713A8AE82B32FA79D5F7FC407D39B",
                verifies: false
            ),
            VectorCase(
                secretKey: nil,
                publicKey: "DFF1D77F2A671C5F36183726DB2341BE58FEAE1DA2DECED843240F7B502BA659",
                auxRand: nil,
                message: "243F6A8885A308D313198A2E03707344A4093822299F31D0082EFA98EC4E6C89",
                signature: "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F69E89B4C5564D00349106B8497785DD7D1D713A8AE82B32FA79D5F7FC407D39B",
                verifies: false
            ),
            VectorCase(
                secretKey: nil,
                publicKey: "DFF1D77F2A671C5F36183726DB2341BE58FEAE1DA2DECED843240F7B502BA659",
                auxRand: nil,
                message: "243F6A8885A308D313198A2E03707344A4093822299F31D0082EFA98EC4E6C89",
                signature: "6CFF5C3BA86C69EA4B7376F31A9BCB4F74C1976089B2D9963DA2E5543E177769FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEBAAEDCE6AF48A03BBFD25E8CD0364141",
                verifies: false
            ),
            VectorCase(
                secretKey: nil,
                publicKey: "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC30",
                auxRand: nil,
                message: "243F6A8885A308D313198A2E03707344A4093822299F31D0082EFA98EC4E6C89",
                signature: "6CFF5C3BA86C69EA4B7376F31A9BCB4F74C1976089B2D9963DA2E5543E17776969E89B4C5564D00349106B8497785DD7D1D713A8AE82B32FA79D5F7FC407D39B",
                verifies: false
            ),
            VectorCase(
                secretKey: "0340034003400340034003400340034003400340034003400340034003400340",
                publicKey: "778CAA53B4393AC467774D09497A87224BF9FAB6F6E68B23086497324D6FD117",
                auxRand: "0000000000000000000000000000000000000000000000000000000000000000",
                message: "",
                signature: "71535DB165ECD9FBBC046E5FFAEA61186BB6AD436732FCCC25291A55895464CF6069CE26BF03466228F19A3A62DB8A649F2D560FAC652827D1AF0574E427AB63",
                verifies: true
            ),
            VectorCase(
                secretKey: "0340034003400340034003400340034003400340034003400340034003400340",
                publicKey: "778CAA53B4393AC467774D09497A87224BF9FAB6F6E68B23086497324D6FD117",
                auxRand: "0000000000000000000000000000000000000000000000000000000000000000",
                message: "11",
                signature: "08A20A0AFEF64124649232E0693C583AB1B9934AE63B4C3511F3AE1134C6A303EA3173BFEA6683BD101FA5AA5DBC1996FE7CACFC5A577D33EC14564CEC2BACBF",
                verifies: true
            ),
            VectorCase(
                secretKey: "0340034003400340034003400340034003400340034003400340034003400340",
                publicKey: "778CAA53B4393AC467774D09497A87224BF9FAB6F6E68B23086497324D6FD117",
                auxRand: "0000000000000000000000000000000000000000000000000000000000000000",
                message: "0102030405060708090A0B0C0D0E0F1011",
                signature: "5130F39A4059B43BC7CAC09A19ECE52B5D8699D1A71E3C52DA9AFDB6B50AC370C4A482B77BF960F8681540E25B6771ECE1E5A37FD80E5A51897C5566A97EA5A5",
                verifies: true
            ),
            VectorCase(
                secretKey: "0340034003400340034003400340034003400340034003400340034003400340",
                publicKey: "778CAA53B4393AC467774D09497A87224BF9FAB6F6E68B23086497324D6FD117",
                auxRand: "0000000000000000000000000000000000000000000000000000000000000000",
                message: "99999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999",
                signature: "403B12B0D8555A344175EA7EC746566303321E5DBFA8BE6F091635163ECA79A8585ED3E3170807E7C03B720FC54C7B23897FCBA0E9D0B4A06894CFD249F22367",
                verifies: true
            ),
        ]

        for vector in vectors {
            runTestVector(vector)
        }
    }

    func testVerifyTweaked() {
        let message = Data("message".utf8)
        let publicKey = Data(
            testHex: "b1ca6327b48b3f2f11c80b460aeff6934cbf1705083792108be9545b53818472"
        )
        let signature = Data(
            testHex: "cddfdf12ffa1698b2fa7449bd6aa4581cdab05205864cddaba1a137a1db132ea2f4255a81199c58241087036f5b66ec4303409cd7d760039729f78f19db004dc"
        )

        _ = schnorrVerify(publicKey, signature, message)
    }
}
