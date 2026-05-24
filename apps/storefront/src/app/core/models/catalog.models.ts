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
