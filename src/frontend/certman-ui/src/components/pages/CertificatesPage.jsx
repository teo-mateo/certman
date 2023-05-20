import React from 'react';
import CACertificatesList from "./CACertificatesList";
import {Routes, Route} from "react-router-dom";
import CACertificateDetails from "./CACertificateDetails";

const CertificatesPage = () => {
    return (
        <div>
            <h1 className="title">Certificates</h1>
            <Routes>
                <Route path="/" element={<CACertificatesList />} />
                <Route path=":id" element={<CACertificateDetails />} />

            </Routes>
        </div>
    );
};

export default CertificatesPage;
