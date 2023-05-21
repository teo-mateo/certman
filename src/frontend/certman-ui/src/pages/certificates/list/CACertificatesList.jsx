import React, { useEffect, useState } from 'react';
import apiService from '../../../services/apiService';
import CreateCACertModal from "./CreateCACertModal";
import {Link} from "react-router-dom";
import DownloadButton from "../components/DownloadButton";

const CACertificatesList = () => {

    const [caCerts, setCaCerts] = useState([]);

    const handleCertCreated = (newCert) => {
        setCaCerts([...caCerts, newCert]);
    }

    useEffect(() => {
        loadCACerts()
    }, []); // Run only once when the component mounts.


    const deleteCert = (caCertId) => {
        if (window.confirm('Are you sure you want to delete this certificate?')) {
            apiService.deleteCACert(caCertId)
                .then(() => loadCACerts())
                .catch(error => console.error('Error:', error));
        }
    };

    const loadCACerts = () => {
        apiService.getCACerts()
            .then(data => setCaCerts(data))
            .catch(error => {
                console.error('There was a problem fetching the data:', error);
            });
    }

    return (
        <div>
            <h4 className="subtitle is-6">Manage and oversee your Root Certificates with simplicity and ease</h4>
            <h2 className="subtitle">Root Certificates</h2>

            <CreateCACertModal onCertCreated={handleCertCreated}/><br/>
            <table className="table is-bordered is-striped is-narrow is-hoverable is-fullwidth">
                <thead>
                <tr>
                    <th>ID</th>
                    <th>Name</th>
                    <th>Keyfile</th>
                    <th>Pemfile</th>
                    <th>Action</th>
                </tr>
                </thead>
                <tbody>
                {caCerts.map(cert => (
                    <tr key={cert.id}>
                        <td>{cert.id}</td>
                        <td><Link to={`/certificates/${cert.id}`}>{cert.name}</Link></td>
                        <td>
                            <DownloadButton certId={cert.id} fileName={cert.keyfile} apiMethod={apiService.getKeyfile} />
                        </td>
                        <td>
                            <DownloadButton certId={cert.id} fileName={cert.pemfile} apiMethod={apiService.getPemfile} />
                        </td>
                        <td>
                            <button className="button is-danger is-small" onClick={() => deleteCert(cert.id)}>Delete</button>
                        </td>
                    </tr>
                ))}
                </tbody>
            </table>
        </div>
    );
};

export default CACertificatesList;
