import React, {useState, useEffect} from 'react';
import './../App.css';
import apiService from './../services/apiService';

const Header = () => {

    const [serverVersion, setServerVersion] = useState('');

    useEffect(() => {
        const fetchServerVersion = async () => {
            try {
                const version = await apiService.getVersion();
                setServerVersion(version);
            } catch (error) {
                console.error('Error fetching server version:', error);
            }
        };

        fetchServerVersion().then();
    }, []);

    return (
        <nav className="navbar" role="navigation" aria-label="main navigation">
            <div className="container">
                <div className="navbar-brand">
                    <a className="navbar-item" href="/">
                    <span className="icon">
                        <i className="fas fa-certificate"></i>
                    </span>
                        <span className="title">Certman {serverVersion}</span>
                    </a>
                </div>
            </div>
        </nav>
    );
};

export default Header;
