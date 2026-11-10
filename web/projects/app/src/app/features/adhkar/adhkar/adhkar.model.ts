export interface AdhkarDto {
  id: string;
  text: string;
  translation: string;
  reference: string;
  category: string;
  displayOrder: number;
  isActive: boolean;
}

export interface AdhkarUpsertRequest {
  text: string;
  translation: string;
  reference: string;
  category: string;
  displayOrder: number;
  isActive: boolean;
}

export const ADHKAR_CATEGORIES = [
  'tasbih',
  'tahmid',
  'takbir',
  'tahlil',
  'hawqala',
  'salawat'
] as const;
