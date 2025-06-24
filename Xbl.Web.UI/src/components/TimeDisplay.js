import React, { useState } from 'react';
import { ToggleButton, ToggleButtonGroup } from '@mui/material';
import './TimeDisplay.css';

const TimeDisplay = ({ value }) => {
  const [unit, setUnit] = useState('minutes');

  const convertValue = () => {
    const days = Math.floor(value / 1440);
    const hours = Math.floor((value % 1440) / 60);
    const minutes = value % 60;
    return { days, hours, minutes };
  };

  const displayDays = () => {
    const { days, hours, minutes } = convertValue();
	if (unit == 'days') return days;
  };

  const displayHours = () => {
    const { days, hours, minutes } = convertValue();
    switch (unit) {
      case 'hours':
        return `${days * 24 + hours}`;
      case 'days':
        return hours;
    }
  };

  const displayMinutes = () => {
    const { days, hours, minutes } = convertValue();
    switch (unit) {
	  case 'minutes':
	    return `${days * 1440 + hours * 60 + minutes}`;
      default:
        return minutes;
    }
  };

  return (
	<div className="time-display">
		<div className="value-container">
			<span className="days"><span className="value">{displayDays()}</span></span>
			<span className="hours"><span className="value">{displayHours()}</span></span>
			<span className="minutes"><span className="value">{displayMinutes()}</span></span>
		</div>
		<div className="toggle-container">
			<ToggleButtonGroup
				color="primary"
				value={unit}
				exclusive
				onChange={(e, newUnit) => {
					if (newUnit != null) setUnit(newUnit);
				}}
				aria-label="time unit"
			>
				<ToggleButton value="minutes">M</ToggleButton>
				<ToggleButton value="hours">H</ToggleButton>
				<ToggleButton value="days">D</ToggleButton>
			</ToggleButtonGroup>
		</div>
	</div>
  );
};

export default TimeDisplay;
