import React from 'react';
import Image from 'next/image';
import './profile.css';

const ProfileSection = () => {
  return (
    <div className="profile-container">
      <Image 
        className="profile-image" 
        src="https://placehold.co/150x150" 
        alt="Profile"
        width={150}
        height={150}
      />
      <div className="profile-info">
        <div className="profile-name-container">
          <div className="profile-name">Alex Johnson</div>
        </div>
        <div className="profile-title-container">
          <div className="profile-title">Full Stack Developer</div>
        </div>
        <div className="profile-description-container">
          <div className="profile-description">
            Crafting digital experiences with modern web technologies. Passionate about<br/>
            creating scalable solutions and beautiful user interfaces.
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProfileSection;