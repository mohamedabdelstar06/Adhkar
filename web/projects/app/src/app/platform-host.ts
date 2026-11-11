export function detectPlatformHost(hostname?: string): boolean {
  const host = hostname ?? (typeof window === 'undefined' ? '' : window.location.hostname);
  const normalized = host.trim().toLowerCase().replace(/\.$/, '');
  return normalized.startsWith('admin.');
}

