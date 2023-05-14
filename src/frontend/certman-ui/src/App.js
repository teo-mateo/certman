import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import LeftNavigation from './components/LeftNavigation';
import CertificatesPage from './components/pages/CertificatesPage';
import SettingsPage from './components/pages/SettingsPage';
import Header from "./components/Header";

const App = () => {
    return (
        <>
            <div className="container">
                <Header/>
                <Router>
                    <div className="columns">
                        <LeftNavigation />
                        <div className="column">
                            <Routes>
                                <Route path="/certificates" element={<CertificatesPage />} />
                                <Route path="/settings" element={<SettingsPage />} />
                            </Routes>
                        </div>
                    </div>
                </Router>
            </div>
        </>
    );
};
export default App;
