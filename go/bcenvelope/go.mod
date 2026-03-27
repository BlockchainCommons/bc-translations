module github.com/nickel-blockchaincommons/bcenvelope-go

go 1.22.0

require (
	github.com/nickel-blockchaincommons/bccomponents-go v0.0.0
	github.com/nickel-blockchaincommons/bcrand-go v0.0.0
	github.com/nickel-blockchaincommons/bctags-go v0.0.0
	github.com/nickel-blockchaincommons/bcur-go v0.0.0
	github.com/nickel-blockchaincommons/dcbor-go v0.0.0
	github.com/nickel-blockchaincommons/knownvalues-go v0.0.0
)

require (
	github.com/btcsuite/btcd/btcec/v2 v2.3.4 // indirect
	github.com/cloudflare/circl v1.6.3 // indirect
	github.com/decred/dcrd/dcrec/secp256k1/v4 v4.0.1 // indirect
	github.com/fxamacker/cbor/v2 v2.9.0 // indirect
	github.com/nickel-blockchaincommons/bccrypto-go v0.0.0 // indirect
	github.com/nickel-blockchaincommons/bcshamir-go v0.0.0 // indirect
	github.com/nickel-blockchaincommons/sskr-go v0.0.0 // indirect
	github.com/x448/float16 v0.8.4 // indirect
	golang.org/x/crypto v0.30.0 // indirect
	golang.org/x/sys v0.28.0 // indirect
	golang.org/x/text v0.21.0 // indirect
)

replace (
	github.com/nickel-blockchaincommons/bccomponents-go => ../bccomponents
	github.com/nickel-blockchaincommons/bccrypto-go => ../bccrypto
	github.com/nickel-blockchaincommons/bcrand-go => ../bcrand
	github.com/nickel-blockchaincommons/bcshamir-go => ../bcshamir
	github.com/nickel-blockchaincommons/bctags-go => ../bctags
	github.com/nickel-blockchaincommons/bcur-go => ../bcur
	github.com/nickel-blockchaincommons/dcbor-go => ../dcbor
	github.com/nickel-blockchaincommons/knownvalues-go => ../knownvalues
	github.com/nickel-blockchaincommons/sskr-go => ../sskr
)
