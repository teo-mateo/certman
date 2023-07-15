const BASE_URL = process.env.REACT_APP_SERVER_URL;

const getBaseUrl = () => {
    return window && window.location && window.location.hostname ? window.location.hostname : BASE_URL;
}

const getVersion = async () => {
    const response = await fetch(`${getBaseUrl()}/api/system/version`);
    if (!response.ok) {
        throw new Error('Error fetching server version');
    }
    const data = await response.json();
    return data.version;
};

const getCACerts = async () => {
    const response = await fetch(`${getBaseUrl()}/api/certs/ca-certs`);
    if (!response.ok) {
        throw new Error('Error fetching CA certificates');
    }
    const data = await response.json();
    return data;
};

const getKeyfile = async (caCertId) => {
    const response = await fetch(`${getBaseUrl()}/api/certs/ca-certs/${caCertId}/keyfile`);
    if (!response.ok) {
        throw new Error(`Error fetching keyfile for cert ID: ${caCertId}`);
    }
    const data = await response.blob();
    return data;
};

const getPemfile = async (caCertId) => {
    const response = await fetch(`${getBaseUrl()}/api/certs/ca-certs/${caCertId}/pemfile`);
    if (!response.ok) {
        throw new Error(`Error fetching pemfile for cert ID: ${caCertId}`);
    }
    const data = await response.blob();
    return data;
};

const downloadLeafCertPfxFile = async (caCertId, id) => {
    const response = await fetch(`${getBaseUrl()}/api/certs/ca-certs/${caCertId}/certs/${id}/pfxfile`);
    if (!response.ok) {
        throw new Error(`Error fetching pfxfile for cert ID: ${id}`);
    }
    const data = await response.blob();
    return data;
}

const downloadLeafCertCrtFile = async (caCertId, id) => {
    const response = await fetch(`${getBaseUrl()}/api/certs/ca-certs/${caCertId}/certs/${id}/crtfile`);
    if (!response.ok) {
        throw new Error(`Error fetching crtfile for cert ID: ${id}`);
    }
    const data = await response.blob();
    return data;
}

// getLeafCertKeyfile
const downloadLeafCertKeyFile = async (caCertId, id) => {
    const response = await fetch(`${getBaseUrl()}/api/certs/ca-certs/${caCertId}/certs/${id}/keyfile`);
    if (!response.ok) {
        throw new Error(`Error fetching keyfile for cert ID: ${id}`);
    }
    const data = await response.blob();
    return data;
}

const createCACert = async (payload) => {
    const response = await fetch(`${getBaseUrl()}/api/certs/ca-certs`, {
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

const getCACertDetails = async (id) => {
    const response = await fetch(`${getBaseUrl()}/api/certs/ca-certs/${id}`);
    if (!response.ok) {
        throw new Error(`Error fetching CA Cert details with id ${id}`);
    }
    const data = await response.json();
    return data;
};

const deleteCert = async (caCertId, certId) => {
    const response = await fetch(`${getBaseUrl()}/api/certs/ca-certs/${caCertId}/certs/${certId}`, { method: 'DELETE' });
    if (!response.ok) {
        throw new Error(`Error deleting Cert with id ${certId}`);
    }
    return;
};

const deleteCACert = async (id) => {
    const response = await fetch(`${getBaseUrl()}/api/certs/ca-certs/${id}`, { method: 'DELETE' });
    if (!response.ok) {
        throw new Error(`Error deleting CA Cert with id ${id}`);
    }
    return;
}

const createLeafCert = async (caCertId, payload) => {
    const response = await fetch(`${getBaseUrl()}/api/certs/ca-certs/${caCertId}/certs`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
    });
    if (!response.ok) {
        throw new Error('Error creating leaf certificate');
    }
    const data = await response.json();
    return data;
};

const getSystemInfo = async () => {
    const response = await fetch(`${getBaseUrl()}/api/system/info`);
    if (!response.ok) {
        throw new Error('Error fetching system info');
    }
    const data = await response.json();
    return data;
}

const apiService = {
    getVersion,
    getCACerts,
    getKeyfile,
    getPemfile,
    downloadLeafCertPfxFile,
    downloadLeafCertKeyFile,
    downloadLeafCertCrtFile,
    createCACert,
    getCACertDetails,
    deleteCACert,
    deleteCert,
    createLeafCert,
    getSystemInfo
};

export default apiService;