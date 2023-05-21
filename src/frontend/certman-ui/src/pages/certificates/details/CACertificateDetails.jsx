import React, { useEffect, useState } from 'react';
import apiService from '../../../services/apiService';
import {Link, useParams} from "react-router-dom";
import CreateLeafCertModal from "./CreateLeafCertModal";

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
            <h2 className="subtitle"><Link to="/certificates">Root Certificates </Link>&gt; <em>{details.name}</em></h2>
            <h4 className="subtitle is-6">Efficiently handle your Leaf Certificates, signed and secured by your trusted Root Certificates</h4>
            <CreateLeafCertModal caCertId={id} onCreated={fetchDetails} />
            <br />
            <div>
                <div className="content">
                    <p><strong>Keyfile:</strong> {details.keyfile}</p>
                    <p><strong>PEM file:</strong> {details.pemfile}</p>
                    <p><strong>Created At:</strong> {new Date(details.createdAt).toLocaleString()}</p>
                </div>

                <h2 className="title is-4">Leaf Certificates</h2>
                <table className="table is-bordered is-striped is-narrow is-hoverable is-fullwidth">
                    <thead>
                    <tr>
                        <th>Name</th>
                        <th>Keyfile</th>
                        <th>CSR File</th>
                        <th>EXT File</th>
                        <th>PFX File</th>
                        <th>Action</th>
                    </tr>
                    </thead>
                    <tbody>
                    {details.certs.map(cert => (
                        <tr key={cert.id}>
                            <td>{cert.name}</td>
                            <td>{cert.keyfile}</td>
                            <td>{cert.csrfile}</td>
                            <td>{cert.extfile}</td>
                            <td>{cert.pfxfile}</td>
                            <td>
                                <button className="button is-danger is-small" onClick={() => deleteCert(details.id, cert.id)}>Delete</button>
                            </td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default CACertificateDetails;