const BASE_URL = "https://localhost:7295";

const getVersion = async () => {
    const response = await fetch(`${BASE_URL}/server/version`);
    if (!response.ok) {
        throw new Error('Error fetching server version');
    }
    const data = await response.json();
    return data.version;
};

const getCACerts = async () => {
    const response = await fetch(`${BASE_URL}/api/certs/ca-certs`);
    if (!response.ok) {
        throw new Error('Error fetching CA certificates');
    }
    const data = await response.json();
    return data;
};

const getKeyfile = async (id) => {
    const response = await fetch(`${BASE_URL}/api/certs/ca-certs/${id}/keyfile`);
    if (!response.ok) {
        throw new Error(`Error fetching keyfile for cert ID: ${id}`);
    }
    const data = await response.blob();
    return data;
};

const getPemfile = async (id) => {
    const response = await fetch(`${BASE_URL}/api/certs/ca-certs/${id}/pemfile`);
    if (!response.ok) {
        throw new Error(`Error fetching pemfile for cert ID: ${id}`);
    }
    const data = await response.blob();
    return data;
};

const createCACert = async (payload) => {
    const response = await fetch(`${BASE_URL}/api/certs/ca-certs`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
    });

    if (!response.ok) {
        throw new Error('Error creating CA cert');
    }

    const data = await response.json();
    return data;
};

const apiService = {
    getVersion,
    getCACerts,
    getKeyfile,
    getPemfile,
    createCACert
};

export default apiService;