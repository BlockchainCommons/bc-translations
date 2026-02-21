module github.com/nickel-blockchaincommons/bctags-go

go 1.21

require github.com/nickel-blockchaincommons/dcbor-go v0.0.0

require (
	github.com/fxamacker/cbor/v2 v2.9.0 // indirect
	github.com/x448/float16 v0.8.4 // indirect
	golang.org/x/text v0.21.0 // indirect
)

replace github.com/nickel-blockchaincommons/dcbor-go => ../dcbor
