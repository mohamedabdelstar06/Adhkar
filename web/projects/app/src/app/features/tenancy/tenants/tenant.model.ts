export interface PlatformTenant {
  tenantId: string;
  slug: string;
  host: string;
  status: string;
  databaseId: string;
  defaultLanguage: string;
  timeZone: string;
}

export interface ProvisionTenantRequest {
  slug: string;
  host: string;
  defaultLanguage: string;
  timeZone: string;
}

export interface UpdatePlatformTenantRequest {
  host: string;
  defaultLanguage: string;
  timeZone: string;
}

export interface OptionItem {
  value: string;
  label: string;
}

export const COMMON_TIME_ZONES: OptionItem[] = [
  { value: 'Africa/Cairo', label: 'Africa/Cairo (مصر - UTC+2 / UTC+3)' },
  { value: 'Asia/Riyadh', label: 'Asia/Riyadh (السعودية - UTC+3)' },
  { value: 'Asia/Dubai', label: 'Asia/Dubai (الإمارات - UTC+4)' },
  { value: 'Asia/Kuwait', label: 'Asia/Kuwait (الكويت - UTC+3)' },
  { value: 'Asia/Amman', label: 'Asia/Amman (الأردن - UTC+3)' },
  { value: 'UTC', label: 'UTC (التوقيت العالمي المنسق)' },
  { value: 'Europe/London', label: 'Europe/London (لندن - UTC+0 / UTC+1)' },
  { value: 'Europe/Paris', label: 'Europe/Paris (باريس - UTC+1 / UTC+2)' },
  { value: 'America/New_York', label: 'America/New_York (نيويورك - UTC-5)' }
];

export const PLATFORM_LANGUAGES: OptionItem[] = [
  { value: 'ar', label: 'العربية (ar)' },
  { value: 'en', label: 'English (en)' }
];

