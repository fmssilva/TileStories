import React from 'react';

// /c:/Users/franc/Desktop/TileStories/src/branding/SiteName.tsx

interface SiteNameProps {
    fontSize?: number;
}

const SiteName: React.FC<SiteNameProps> = ({ fontSize = 24 }) => {
    return (
        <div
            className="font-bold whitespace-nowrap"
            style={{ fontSize: `${fontSize}px`, lineHeight: 1 }}
        >
            <span style={{ color: '#1e5a96' }}>Tile</span>
            <span style={{ color: '#d4a837' }}>Stories</span>
        </div>
    );
};

export default SiteName;
