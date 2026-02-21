package dcbor

import "testing"

func assertActualExpected(t *testing.T, actual, expected string) {
	t.Helper()
	if actual != expected {
		t.Logf("Actual:\n%s", actual)
		t.Logf("Expected:\n%s", expected)
		t.Fatalf("actual text does not match expected text")
	}
}
