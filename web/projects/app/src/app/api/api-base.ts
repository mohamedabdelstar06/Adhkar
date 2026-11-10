export const API_BASE_URL: string =
  typeof window !== 'undefined' && (window as unknown as { __ZAD_API_URL__?: string }).__ZAD_API_URL__
    ? (window as unknown as { __ZAD_API_URL__?: string }).__ZAD_API_URL__!
    : '';

export const API_ROUTES = {
  adhkar: `${API_BASE_URL}/api/adhkar`,
  catalog: `${API_BASE_URL}/api/catalog`,
  orders: `${API_BASE_URL}/api/orders`,
  tenancy: `${API_BASE_URL}/api/tenancy`,
  platformTenants: `${API_BASE_URL}/api/platform/tenants`
} as const;
