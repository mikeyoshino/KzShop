declare global {
  interface Window {
    __toyshopsRuntimeConfig?: Record<string, never>;
  }
}

export {};
