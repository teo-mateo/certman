import React, { useEffect, useState } from 'react';
import apiService from './../../services/apiService';
import {Link, useParams} from "react-router-dom";

const CACertificateDetails = () => {

    const [details, setDetails] = useState(null);
    const { id } = useParams();

    const deleteCert = (caCertId, certId) => {
        if (window.confirm('Are you sure you want to delete this certificate?')) {
            apiService.deleteCert(caCertId, certId)
                .then(() => fetchDetails())
                .catch(error => console.error('Error:', error));
        }
    };

    useEffect(() => fetchDetails, [id]);

    const fetchDetails = () => {
        apiService.getCACertDetails(id)
            .then(data => setDetails(data))
            .catch(error => console.error('Error:', error));
    };

    if (!details) {
        return <div>Loading...</div>;
    }

    return (
        <div>
            <h2 className="subtitle"><Link to="/certificates">CA Certificates </Link>&gt; <em>{details.name}</em></h2>

        <div className="container">
            <div className="card">
                <header className="card-header">
                    <p className="card-header-title">{details.name}</p>
                </header>
                <div className="card-content">
                    <div className="content">
                        <p><strong>Keyfile:</strong> {details.keyfile}</p>
                        <p><strong>PEM File:</strong> {details.pemfile}</p>
                        <p><strong>Created At:</strong> {new Date(details.createdAt).toLocaleString()}</p>
                    </div>
                </div>
                <footer className="card-footer">
                    <p className="card-footer-item">ID: {details.id}</p>
                </footer>
            </div>
            <br />
            <h2 className="subtitle is-small">Certs</h2>

            {details.certs.map(cert => (
                <>
                <div key={cert.id} className="card" >
                    <header className="card-header">
                        <p className="card-header-title">{cert.name}</p>
                        <button onClick={() => deleteCert(details.id, cert.id)} className="delete" aria-label="close"></button>
                    </header>
                    <div className="card-content">
                        <div className="content">
                            <p><strong>Keyfile:</strong> {cert.keyfile}</p>
                            <p><strong>CSR File:</strong> {cert.csrfile}</p>
                            <p><strong>EXT File:</strong> {cert.extfile}</p>
                            <p><strong>PFX File:</strong> {cert.pfxfile}</p>
                            <p><strong>Created At:</strong> {new Date(cert.createdAt).toLocaleString()}</p>
                        </div>
                    </div>
                    <footer className="card-footer">
                        <p className="card-footer-item">ID: {cert.id}</p>
                    </footer>
                </div>
                    <br/>
                </>
            ))}
        </div>
        </div>
    );
}

export default CACertificateDetails;