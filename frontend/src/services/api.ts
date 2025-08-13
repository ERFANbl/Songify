import axios from "axios";

const api = axios.create({
    baseURL: 'http://127.0.0.1:5143',
})

api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token')
    if (token) {
        config.headers.Authorization = `Bearer ${token}`
    }
    return config
})

export interface AuthResponse {
    Success: boolean;
    Token?: string;
    Message?: string;
}

export const signup = (data: {username: string, password: string}) => api.post<AuthResponse>('/api/Auth/signup', data)

export const login = (data: {username: string, password: string}) => api.post<AuthResponse>('/api/Auth/login', data)

export const logout = () => api.post('/api/Auth/logout')

export default api;