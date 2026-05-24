declare global {
  interface Window {
    __supabaseUrl?: string;
    __supabaseAnonKey?: string;
  }
}

export {};
