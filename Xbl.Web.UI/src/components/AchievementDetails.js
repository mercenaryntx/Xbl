// src/components/AchievementDetails.js
import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import TimeDisplay from './TimeDisplay';
import gamerscoreIcon from '../assets/images/gamerscore.svg';
import trophyIcon from '../assets/images/icons8-trophy-16.png';
import diamondIcon from '../assets/images/icons8-diamond-16.png';
import lockedIcon from '../assets/images/locked.png';
import placeholderIcon from '../assets/images/placeholder.png';
import './AchievementDetails.css';

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;

const AchievementDetails = () => {
  const { source, titleId } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const title = location.state?.game;
  const [minutes, setMinutes] = useState(null);
  const [achievements, setAchievements] = useState([]);

  useEffect(() => {
    const fetchAchievements = async () => {
      try {
        const response = await fetch(`${API_BASE_URL}/Titles/${source}/${titleId}`);
        const data = await response.json();
        setAchievements(data.achievements);
		setMinutes(data.minutes);
      } catch (error) {
        console.error('Error fetching achievement details:', error);
      }
    };

    fetchAchievements();
  }, [titleId]);
  
  function resize(url) {
	  if (url) {
		  if (source == 'live') return url + '&w=400';
		  return url;
	  }
	  return placeholderIcon;
  }

  return (
	<div className="achievement-details">
		{title &&
		<div className="title">
			<button id="back" onClick={() => navigate(-1)}>&#129168;</button>
			<img src={title.displayImage} alt={title.name} className="game-image" />
			<div className="game-details">
				<div className="game-title">
					<h3>{title.name}</h3>
					{minutes > 0 && <TimeDisplay value={minutes}/>}
				</div>
				<div className="stat">
					<span className="nums">
						<span className="gamerscore"><img src={gamerscoreIcon} className="icon" /> {title.currentGamerscore}/{title.totalGamerscore}</span>
						<span className="achievements"><img src={trophyIcon} className="icon" /> {title.currentAchievements}</span>
					</span>
					<span className="percentage">{title.progressPercentage.toFixed(2)}%</span>
				</div>
				<div className="progress-bar">
					<div className="progress" style={{ width: `${title.progressPercentage}%` }}></div>
				</div>
			</div>
		</div>
		}
		<div className="achievement-wrap grid-container">
		{achievements.map((achievement) => (
		<div key={achievement.id} className="achievement-item grid-row-item-d4-t8-m4">
			<div className="achievement-container">
			{achievement.isUnlocked &&
			<img src={resize(achievement.displayImage)} alt={achievement.title} className={source} />
			}
			{!achievement.isUnlocked &&
			<img src={lockedIcon} alt={achievement.title} className="achievement-image" />
			}
			<h3 className={achievement.isSecret.toString()}>{achievement.name}</h3>
			<div className="achievement-info">
				<span className="gamerscore">{achievement.isRare && <img src={diamondIcon} className="icon" /> }<img src={gamerscoreIcon} className="icon" /> {achievement.gamerscore}</span>
				<p className="percentage"> {achievement.rarityPercentage}%</p>
			</div>
			</div>
		</div>
		))}
		</div>
	</div>
  );
};

export default AchievementDetails;
