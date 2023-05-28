import React from 'react';
import CACertificatesList from "./list/CACertificatesList";
import {Routes, Route} from "react-router-dom";
import CACertificateDetails from "./details/CACertificateDetails";

const CertificatesPage = () => {
    return (
        <div>
            <h1 className="title">Certificates</h1>
            <Routes>
                <Route path="/" element={<CACertificatesList />} />
                <Route path="/:id" element={<CACertificateDetails />} />
            </Routes>
        </div>
    );
};

export default CertificatesPage;
