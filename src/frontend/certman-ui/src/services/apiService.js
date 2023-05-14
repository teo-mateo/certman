const BASE_URL = "https://localhost:7295";

const getVersion = async () => {
    const response = await fetch(`${BASE_URL}/server-version`);
    if (!response.ok) {
        throw new Error('Error fetching server version');
    }
    const data = await response.json();
    return data.serverVersion;
};

const apiService = {
    getVersion,
};

export default apiService;