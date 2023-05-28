import React, { useEffect, useState } from 'react';
import apiService from '../../../services/apiService';
import {Link, useParams} from "react-router-dom";
import CreateLeafCertModal from "./CreateLeafCertModal";
import DownloadButton from "../components/DownloadButton";

const CACertificateDetails = () => {

    console.log('CACertificateDetails component');

    const [details, setDetails] = useState(null);
    const { id } = useParams();

    console.log('CACertificateDetails id:', id);

    const deleteCert = (caCertId, certId) => {
        if (window.confirm('Are you sure you want to delete this certificate?')) {
            apiService.deleteCert(caCertId, certId)
                .then(() => fetchDetails())
                .catch(error => console.error('Error:', error));
        }
    };

    useEffect(() => {
        fetchDetails();
    }, []);

    const fetchDetails = () => {
        console.log('fetching details');
        apiService.getCACertDetails(id)
            .then(data => {
                console.log('fetched details', data);
                setDetails(data);

            })
            .catch(error => console.error('Error:', error));
    };

    if (!details) {
        return <div>Loading...</div>;
    }

    return (
        <div>
            <h4 className="subtitle is-6">Efficiently handle your Leaf Certificates, signed and secured by your trusted Root Certificates</h4>

            <h2 className="subtitle"><Link to="/certificates">Root Certificates </Link>&gt; <em>{details.name}</em></h2>
            <div>
                <div className="content quote-div">
                    <p><strong>Keyfile:</strong> <code>{details.keyfile}</code></p>
                    <p><strong>PEM file:</strong> <code>{details.pemfile}</code></p>
                    <p><strong>Created At:</strong> <code> {new Date(details.createdAt).toLocaleString()}</code></p>
                </div>

                <br />
                <h2 className="title is-5">Leaf Certificates</h2>
                <CreateLeafCertModal caCertId={id} onCreated={fetchDetails} />
                <br />
                <table className="table is-bordered is-striped is-narrow is-hoverable is-fullwidth">
                    <thead>
                    <tr>
                        <th>Name</th>
                        <th>Keyfile</th>
                        <th>CSR File</th>
                        <th>EXT File</th>
                        <th>CRT File</th>
                        <th>PFX File</th>
                        <th>Action</th>
                    </tr>
                    </thead>
                    <tbody>
                    {details.certs.length === 0 && (
                        <tr>
                            <td colSpan="6" style={{textAlign: "center"}}>No leaf certificates found.</td>
                        </tr>
                    )}
                    {details.certs.map(cert => (
                        <tr key={cert.id} style={{verticalAlign: "middle"}}>
                            <td style={{verticalAlign: "middle"}}>{cert.name}</td>
                            <td style={{verticalAlign: "middle"}}><DownloadButton caCertId={cert.caCertId} certId={cert.id} fileName={cert.keyfile} apiMethod={apiService.downloadLeafCertKeyFile} /> </td>
                            <td style={{verticalAlign: "middle"}}>{cert.csrfile}</td>
                            <td style={{verticalAlign: "middle"}}>{cert.extfile}</td>
                            <td style={{verticalAlign: "middle"}}><DownloadButton caCertId={cert.caCertId} certId={cert.id} fileName={cert.crtfile} apiMethod={apiService.downloadLeafCertCrtFile} /> </td>
                            <td style={{verticalAlign: "middle"}}><DownloadButton caCertId={cert.caCertId} certId={cert.id} fileName={cert.pfxfile} apiMethod={apiService.downloadLeafCertPfxFile} /> </td>
                            <td style={{verticalAlign: "middle"}}>
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