import React, { useEffect, useState } from 'react';
import apiService from './../../services/apiService';
import CreateCertModal from "./CreateCertModal";

const CACertificatesList = () => {

    const [caCerts, setCaCerts] = useState([]);

    const downloadFile = async (id, fileName, apiMethod) => {
        const fileBlob = await apiMethod(id);
        const url = window.URL.createObjectURL(fileBlob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', fileName);
        document.body.appendChild(link);
        link.click();
        link.remove();
    };

    const handleCertCreated = (newCert) => {
        setCaCerts([...caCerts, newCert]);
    }

    useEffect(() => {
        apiService.getCACerts()
            .then(data => setCaCerts(data))
            .catch(error => {
                console.error('There was a problem fetching the data:', error);
            });
    }, []); // Run only once when the component mounts.

    const tableStyle = {
        border: '1px solid black',
        borderCollapse: 'collapse',
        width: '100%',
        textAlign: 'left',
    };

    const thTdStyle = {
        border: '1px solid black',
        padding: '8px',
    };

    return (
        <div>
            <h2 className="subtitle">CA Certificates</h2>
            <p>
                <CreateCertModal onCertCreated={handleCertCreated}/>
            </p><br/>
            <table style={tableStyle}>
                <thead>
                <tr>
                    <th style={thTdStyle}>ID</th>
                    <th style={thTdStyle}>Name</th>
                    <th style={thTdStyle}>Keyfile</th>
                    <th style={thTdStyle}>Pemfile</th>
                    <th style={thTdStyle}>Created At</th>
                </tr>
                </thead>
                <tbody>
                {caCerts.map(cert => (
                    <tr key={cert.id}>
                        <td style={thTdStyle}>{cert.id}</td>
                        <td style={thTdStyle}>{cert.name}</td>
                        <td style={thTdStyle}>
                            <button onClick={() => downloadFile(cert.id, cert.keyfile, apiService.getKeyfile)}>
                                {cert.keyfile}
                            </button>
                        </td>
                        <td style={thTdStyle}>
                            <button onClick={() => downloadFile(cert.id, cert.pemfile, apiService.getPemfile)}>
                                {cert.pemfile}
                            </button>
                        </td>
                        <td style={thTdStyle}>{new Date(cert.createdAt).toLocaleString()}</td>
                    </tr>
                ))}
                </tbody>
            </table>
        </div>
    );
};

export default CACertificatesList;
