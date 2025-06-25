import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import AchievementsList from './components/AchievementsList';
import AchievementDetails from './components/AchievementDetails';
import './App.css';

function App() {
  return (
	<Router>
		<div className="app">
			<Routes>
				<Route path="/" element={<AchievementsList />} />
				<Route path="/details/:source/:titleId" element={<AchievementDetails />} />
			</Routes>
		</div>
	</Router>
  );
}

export default App;
