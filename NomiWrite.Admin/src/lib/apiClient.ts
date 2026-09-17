import axios from 'axios';

const API_BASE_URL = 'http://localhost:5097/api/admin';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor to attach the JWT token to requests
apiClient.interceptors.request.use(
  (config) => {
    // Usually retrieved from localStorage, sessionStorage, or a global state (e.g., Zustand/Redux)
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Optional: Interceptor to handle global errors (like 401 Unauthorized)
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      console.warn("Unauthorized access. Redirecting to login...");
      // Add logic to redirect to login or clear token here.
    }
    return Promise.reject(error);
  }
);
