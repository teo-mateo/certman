import React, {useEffect, useState} from 'react';
import apiService from "../../services/apiService";

const CertificatesPage = () => {

    const [systemInfo, setSystemInfo] = useState({});

    useEffect(() => {
        apiService.getSystemInfo()
            .then(data => setSystemInfo(data))
            .catch(error => {
                console.error('There was a problem fetching the data:', error);
            });
    }, []);

    return (
        <div>
            <h1 className="title">Settings</h1>
            <div>
                <h2 className="subtitle">Server settings</h2>
                <table className="table is-bordered is-striped is-narrow is-hoverable is-fullwidth">
                    <thead>
                    <tr>
                        <th>Property</th>
                        <th>Value</th>
                    </tr>
                    </thead>
                    <tbody>
                    {Object.keys(systemInfo).map(key => (
                        <tr key={key}>
                            <td>{key}</td>
                            <td>{systemInfo[key]}</td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            </div>

        </div>
    );
};

export default CertificatesPage;
