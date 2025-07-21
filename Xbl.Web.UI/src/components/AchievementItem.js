// src/components/AchievementItem.js
import React from 'react';
import { useNavigate } from 'react-router-dom';
import './AchievementItem.css';
import gamerscoreIcon from '../assets/images/gamerscore.svg';
import trophyIcon from '../assets/images/icons8-trophy-16.png';

const AchievementItem = ({ game, source }) => {
  const { name, titleId, currentGamerscore, totalGamerscore, currentAchievements, progressPercentage } = game;
  const navigate = useNavigate();
  const handleClick = () => {
    navigate(`/details/${source}/${titleId}`, { state: { game }});
  };

  function displayImage(id) {
      return `https://xblcdn.blob.core.windows.net/titles/${id}.png`;
  }

  return (
    <div className="title" onClick={handleClick}>
      <img src={displayImage(titleId)} alt={name} className="game-image" />
      <div className="game-details">
		<div className="game-title">
			<h3>{name}</h3>
		</div>	
        <div className="stat">
		  <span className="nums">
			  <span className="gamerscore"><img src={gamerscoreIcon} alt="gamerscore" className="icon" /> {currentGamerscore}/{totalGamerscore}</span>
			  <span className="achievements"><img src={trophyIcon} alt="trophy" className="icon" /> {currentAchievements}</span>
		  </span>
          <span className="percentage">{progressPercentage.toFixed(2)}%</span>
        </div>
        <div className="progress-bar">
          <div className="progress" style={{ width: `${progressPercentage}%` }}></div>
        </div>
      </div>
    </div>
  );
};

export default AchievementItem;
