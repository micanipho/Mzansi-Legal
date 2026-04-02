export const OFFLINE_ERROR_MESSAGE =
  "You appear to be offline. Reconnect and try again.";

function isNavigatorOffline(): boolean {
  return typeof navigator !== "undefined" && navigator.onLine === false;
}

export function isOfflineError(error: unknown): boolean {
  if (isNavigatorOffline()) {
    return true;
  }

  if (error instanceof TypeError) {
    return true;
  }

  if (error instanceof Error) {
    return /failed to fetch|networkerror|load failed/i.test(error.message);
  }

  return false;
}

export function getUserFacingErrorMessage(
  error: unknown,
  fallbackMessage: string,
): string {
  if (isOfflineError(error)) {
    return OFFLINE_ERROR_MESSAGE;
  }

  return fallbackMessage;
}
