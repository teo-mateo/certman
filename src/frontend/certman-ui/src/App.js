import React from 'react';
import {BrowserRouter as Router, Navigate,  Route, Routes} from 'react-router-dom';
import LeftNavigation from './components/LeftNavigation';
import CertificatesPage from './pages/certificates/CertificatesPage';
import SettingsPage from './pages/settings/SettingsPage';
import Header from "./components/Header";

const App = () => {

    return (
        <>
            <div className="container">
                <Header/>
                <Router>
                    <Routes>
                        <Route path="/" element={<Navigate to="/certificates" />} />
                        <Route path="/index.html" element={<Navigate to="/certificates" />} />
                    </Routes>
                    <div className="columns">
                        <LeftNavigation />
                        <div className="column">
                            <Routes>
                                <Route path="/certificates/*" element={<CertificatesPage />} />
                                <Route path="/settings" element={<SettingsPage />} />
                                <Route path="/" element={<Navigate to="/certificates" />} />
                            </Routes>
                        </div>
                    </div>
                </Router>
            </div>
        </>
    );
};
export default App;
