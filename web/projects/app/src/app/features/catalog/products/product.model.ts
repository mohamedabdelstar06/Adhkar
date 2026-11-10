export interface ProductDto {
  id: string;
  sku: string;
  name: string;
  description: string;
  unitPrice: number;
  stockQuantity: number;
  isActive: boolean;
}

export interface ProductListItemDto {
  id: string;
  sku: string;
  name: string;
  unitPrice: number;
  stockQuantity: number;
  isActive: boolean;
}

export interface CreateProductRequest {
  sku: string;
  name: string;
  description: string;
  unitPrice: number;
  stockQuantity: number;
}

export interface UpdateProductRequest {
  name: string;
  description: string;
  unitPrice: number;
}

export interface PagedList<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

