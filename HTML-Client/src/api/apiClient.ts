import axios from "axios";

export const BACKEND_URL = "https://localhost:7072/api";

const apiClient = axios.create({
    baseURL: BACKEND_URL,
    headers: {
        "Content-Type" : "application/json",
    },
});

export default apiClient;