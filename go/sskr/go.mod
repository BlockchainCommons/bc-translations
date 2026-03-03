module github.com/nickel-blockchaincommons/sskr-go

go 1.21

require (
	github.com/nickel-blockchaincommons/bcrand-go v0.0.0
	github.com/nickel-blockchaincommons/bcshamir-go v0.0.0
)

require (
	github.com/btcsuite/btcd/btcec/v2 v2.3.4 // indirect
	github.com/decred/dcrd/dcrec/secp256k1/v4 v4.0.1 // indirect
	github.com/nickel-blockchaincommons/bccrypto-go v0.0.0 // indirect
	golang.org/x/crypto v0.24.0 // indirect
	golang.org/x/sys v0.21.0 // indirect
)

replace (
	github.com/nickel-blockchaincommons/bccrypto-go => ../bccrypto
	github.com/nickel-blockchaincommons/bcrand-go => ../bcrand
	github.com/nickel-blockchaincommons/bcshamir-go => ../bcshamir
)
