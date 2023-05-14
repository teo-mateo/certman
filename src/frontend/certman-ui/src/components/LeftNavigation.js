import React from 'react';
import { NavLink } from 'react-router-dom';

const LeftNavigation = () => {
    return (
        <div className="column is-one-fifth">
            <aside className="menu">
                <ul className="menu-list">
                    <li>
                        <NavLink to="/certificates" activeclassname="is-active">Certificates</NavLink>
                    </li>
                    <li>
                        <NavLink to="/settings" activeclassname="is-active">Settings</NavLink>
                    </li>
                </ul>
            </aside>
        </div>
    );
};

export default LeftNavigation;
