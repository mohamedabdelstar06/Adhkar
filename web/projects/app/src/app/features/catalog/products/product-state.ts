import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { API_ROUTES } from '../../../api/api-base';
import {
  CreateProductRequest,
  PagedList,
  ProductDto,
  ProductListItemDto,
  UpdateProductRequest
} from './product.model';

@Injectable()
export class ProductState {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = API_ROUTES.catalog;

  readonly products = signal<ProductListItemDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly createOpen = signal(false);
  readonly editingProduct = signal<ProductDto | null>(null);
  readonly errorMessage = signal('');
  readonly searchQuery = signal('');

  readonly filteredProducts = computed(() => {
    const query = this.searchQuery().trim().toLowerCase();
    const list = this.products();
    if (!query) {
      return list;
    }

    return list.filter(
      (item) => item.name.toLowerCase().includes(query) || item.sku.toLowerCase().includes(query)
    );
  });

  readonly activeCount = computed(() => this.products().filter((item) => item.isActive).length);

  readonly totalStock = computed(() =>
    this.products().reduce((sum, item) => sum + item.stockQuantity, 0)
  );

  loadProducts(page = 1, pageSize = 20): void {
    this.loading.set(true);
    this.http
      .get<PagedList<ProductListItemDto>>(`${this.baseUrl}/products?page=${page}&pageSize=${pageSize}`)
      .subscribe({
        next: (response) => {
          this.products.set(response.items);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
  }

  openCreate(): void {
    this.errorMessage.set('');
    this.editingProduct.set(null);
    this.createOpen.set(true);
  }

  closeCreate(): void {
    this.createOpen.set(false);
    this.errorMessage.set('');
  }

  openUpdate(item: ProductListItemDto): void {
    this.errorMessage.set('');
    this.createOpen.set(false);
    this.editingProduct.set({
      id: item.id,
      sku: item.sku,
      name: item.name,
      description: '',
      unitPrice: item.unitPrice,
      stockQuantity: item.stockQuantity,
      isActive: item.isActive
    });

    this.http.get<ProductDto>(`${this.baseUrl}/products/${item.id}`).subscribe({
      next: (details) => {
        if (this.editingProduct()?.id === item.id) {
          this.editingProduct.set(details);
        }
      },
      error: () => {
        // Fallback to list row values if details fetch fails.
      }
    });
  }

  closeUpdate(): void {
    this.editingProduct.set(null);
    this.errorMessage.set('');
  }

  createProduct(request: CreateProductRequest): void {
    this.saving.set(true);
    this.errorMessage.set('');
    this.http.post<ProductDto>(`${this.baseUrl}/products`, request).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeCreate();
        this.loadProducts();
      },
      error: (err: { error?: { title?: string } }) => {
        this.saving.set(false);
        this.errorMessage.set(err.error?.title || 'Error creating product');
      }
    });
  }

  updateProduct(id: string, request: UpdateProductRequest): void {
    this.saving.set(true);
    this.errorMessage.set('');
    this.http.put<void>(`${this.baseUrl}/products/${id}`, request).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeUpdate();
        this.loadProducts();
      },
      error: (err: { error?: { title?: string } }) => {
        this.saving.set(false);
        this.errorMessage.set(err.error?.title || 'Error updating product');
      }
    });
  }

  deactivateProduct(id: string): void {
    this.http.post<void>(`${this.baseUrl}/products/${id}/deactivate`, {}).subscribe({
      next: () => this.loadProducts()
    });
  }
}

