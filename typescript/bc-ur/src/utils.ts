/**
 * Returns true if the character is valid in a UR type (lowercase alpha, digit, or hyphen).
 */
export const isUrTypeChar = (value: string): boolean => {
  if (value.length !== 1) {
    return false;
  }

  const code = value.charCodeAt(0);
  if (code >= 0x61 && code <= 0x7a) {
    return true;
  }
  if (code >= 0x30 && code <= 0x39) {
    return true;
  }
  return code === 0x2d;
};

/**
 * Returns true if all characters in the string are valid UR type characters.
 */
export const isUrType = (value: string): boolean => {
  if (value.length === 0) {
    return false;
  }
  for (const char of value) {
    if (!isUrTypeChar(char)) {
      return false;
    }
  }
  return true;
};

/**
 * Extracts a human-readable message from an unknown thrown value.
 */
export const messageFromUnknown = (error: unknown): string => {
  if (error instanceof Error) {
    return error.message;
  }
  return String(error);
};
