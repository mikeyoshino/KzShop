export interface ProductCard {
  id: string;
  name: string;
  slug: string;
  categorySlug: string;
  studioName: string;
  priceAmount: number;
  currency: string;
  scale: string;
  status: string;
  isNew: boolean;
  primaryImageUrl?: string | null;
}

export interface ProductImage {
  id: string;
  publicUrl: string;
  altText: string;
  sortOrder: number;
}

export interface ProductSpecification {
  label: string;
  value: string;
  sortOrder: number;
}

export interface ProductDetail {
  id: string;
  name: string;
  slug: string;
  studioName: string;
  categorySlug: string;
  priceAmount: number;
  currency: string;
  description: string;
  shortDescription: string;
  scale: string;
  materials: string;
  dimensionsText: string;
  editionSize: string;
  estimatedReleaseText: string;
  status: string;
  isNew: boolean;
  images: ProductImage[];
  specifications: ProductSpecification[];
}

export interface PagedProductsResponse {
  items: ProductCard[];
}

export interface StudioSummary {
  id: string;
  name: string;
  slug: string;
  isActive: boolean;
}

export interface SearchStudiosResponse {
  items: StudioSummary[];
}

export interface CategorySummary {
  id: string;
  name: string;
  slug: string;
  description: string;
  isActive: boolean;
}

export enum ProductStatus {
  PreOrder = 'PreOrder',
  Active = 'Active',
  Waitlist = 'Waitlist',
  Archived = 'Archived',
}

export interface AdminProductSpecification {
  id: string;
  label: string;
  value: string;
  sortOrder: number;
}

export interface AdminProductImage {
  id: string;
  publicUrl: string;
  altText: string;
  sortOrder: number;
}

export interface AdminProductDetail {
  id: string;
  name: string;
  slug: string;
  shortDescription: string;
  description: string;
  categoryId: string;
  categoryName: string;
  studioId: string;
  studioName: string;
  sku: string;
  priceAmount: number;
  currency: string;
  status: ProductStatus;
  isNew: boolean;
  isFeatured: boolean;
  scale: string;
  materials: string;
  dimensionsText: string;
  editionSize: string;
  estimatedReleaseText: string;
  inventoryOnHand: number;
  inventoryReserved: number;
  inventoryAvailable: number;
  images: AdminProductImage[];
  specifications: AdminProductSpecification[];
}

export interface CreateProductSpecificationInput {
  label: string;
  value: string;
  sortOrder: number;
}

export interface UpsertProductInput {
  name: string;
  slug: string;
  shortDescription: string;
  description: string;
  categoryId: string;
  studioId: string;
  sku: string;
  priceAmount: number;
  currency: string;
  status: ProductStatus;
  isNew: boolean;
  isFeatured: boolean;
  scale: string;
  materials: string;
  dimensionsText: string;
  editionSize: string;
  estimatedReleaseText: string;
  specifications: CreateProductSpecificationInput[];
}
