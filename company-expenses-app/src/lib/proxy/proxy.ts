import axios, { type AxiosInstance, type AxiosRequestConfig, type AxiosError } from "axios";

/**
 * API Proxy class for making HTTP requests to the backend
 * Handles authentication, error handling, and request/response interceptors
 */
export class ApiProxy {
  private axiosInstance: AxiosInstance;

  constructor(baseURL?: string, timeout?: number) {
    this.axiosInstance = axios.create({
      baseURL: baseURL || import.meta.env.VITE_API_BASE_URL || "https://localhost:7200",
      timeout: timeout || Number(import.meta.env.VITE_API_TIMEOUT) || 30000,
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      withCredentials: true, // Critical: sends cookies with every request
    });

    // Request interceptor - add auth tokens, modify headers
    this.axiosInstance.interceptors.request.use(
      (config) => {
        console.log(`🌐 ${config.method?.toUpperCase()} ${config.url}`);
        console.log(`   withCredentials: ${config.withCredentials}`);

        return config;
      },
      (error) => {
        console.error("Request error:", error);
        return Promise.reject(error);
      }
    );

    // Response interceptor - handle errors, refresh tokens
    this.axiosInstance.interceptors.response.use(
      (response) => {
        console.log(`✅ ${response.config.method?.toUpperCase()} ${response.config.url} → ${response.status}`);
        return response;
      },
      (error: AxiosError) => {
        if (error.response) {
          console.log(`❌ ${error.config?.method?.toUpperCase()} ${error.config?.url} → ${error.response.status}`);

          switch (error.response.status) {
            case 401:
              break;
            case 403:
              console.error("Forbidden - insufficient permissions");
              break;
            case 404:
              console.error("Resource not found:", error.config?.url);
              break;
            case 500:
              console.error("Server error");
              break;
            default:
              console.error("API Error:", error.response.status);
          }
        } else if (error.request) {
          console.error("No response from server - check if API is running");
        } else {
          console.error("Request setup error:", error.message);
        }

        return Promise.reject(error);
      }
    );
  }

  
  async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.axiosInstance.get<T>(url, config);
    return response.data;
  }

  async post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.axiosInstance.post<T>(url, data, config);
    return response.data;
  }

  async put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.axiosInstance.put<T>(url, data, config);
    return response.data;
  }

  async patch<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.axiosInstance.patch<T>(url, data, config);
    return response.data;
  }

  async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.axiosInstance.delete<T>(url, config);
    return response.data;
  }
}

// Export singleton instance with default configuration
export const apiProxy = new ApiProxy();

// Export class for creating custom instances if needed
export default apiProxy;
