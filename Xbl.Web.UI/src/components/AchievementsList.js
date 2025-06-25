// src/components/AchievementsList.js
import React, { useState, useEffect, useRef } from 'react';
import InfiniteScroll from 'react-infinite-scroll-component';
import AchievementItem from './AchievementItem';
import searchIcon from '../assets/images/icons8-search-16.png';
import './AchievementsList.css';

const AchievementsList = () => {
  const [games, setGames] = useState([]);
  const [hasMore, setHasMore] = useState(true);
  const [page, setPage] = useState(0);
  const [order, setOrder] = useState('lastPlayed-desc');
  const [source, setSource] = useState('live');
  const [searchVisible, setSearchVisible] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const searchInputRef = useRef(null);
  const searchToggleRef = useRef(null);

  useEffect(() => {
    fetchMoreData();
  }, [order, source, searchQuery]);
  
  function replaceOrderPostfix(str) {
    return str.replace(/(.*)-(asc|desc)$/, (match, p1, p2) => {
      return `${p1}&orderDir=${p2.toUpperCase()}`;
    });
  }

  const fetchMoreData = async () => {
	const o = replaceOrderPostfix(order);
    const response = await fetch(`https://localhost:7238/Titles/${source}?page=${page}&orderBy=${o}&title=${searchQuery}`);
    const data = await response.json();
    setGames([...games, ...data]);
    setPage(page + 1);
    if (data.length < 50) {
      setHasMore(false);
    }
  };
  
  const handleOrderChange = (e) => {
    setOrder(e.target.value);
    setGames([]);
    setPage(0);
    setHasMore(true);
  };

  const handleSourceChange = (e) => {
    setSource(e.target.value);
    setGames([]);
    setPage(0);
    setHasMore(true);
  };

  const handleSearchToggle = () => {
    setSearchVisible(!searchVisible);	
	if (!searchVisible) {
      setTimeout(() => {
        searchInputRef.current.focus();
      }, 0);
    }
  };

  const handleSearchChange = (e) => {
    setSearchQuery(e.target.value);
    setGames([]);
    setPage(0);
    setHasMore(true);
  };

  const handleSearchBlur = (e) => {
    if (e.relatedTarget !== searchToggleRef.current) {
      setSearchVisible(false);
    }
	e.stopPropagation();
  };
 
  const update = async (e) => {
	const response = await fetch(`https://localhost:7238/Update`);
  }

  return (
	<div>
	<div className="order-selection">
		{searchVisible && (
		<input id="search"
			type="text"
			value={searchQuery}
			onChange={handleSearchChange}
			onBlur={handleSearchBlur}
			placeholder="Search game title"
			ref={searchInputRef}
		/>
		)}
		<button id="search-toggle" onClick={handleSearchToggle} ref={searchToggleRef}>
			<img src={searchIcon} aria-label="search"></img>
		</button>
		<button id="update" onClick={update}>Update</button>
		<select id="order" value={order} onChange={handleOrderChange}>
			<option value="lastPlayed-desc">Recently played</option>
			<option value="name-asc">A-Z</option>
			<option value="name-desc">Z-A</option>
			<option value="progress-desc">Most completed</option>
			<option value="progress-asc">Least completed</option>
		</select>
		<select id="source" value={source} onChange={handleSourceChange}>
			<option value="live">Live</option>
			<option value="x360">Xbox 360</option>
		</select>
	</div>
	<InfiniteScroll
		dataLength={games.length}
		next={fetchMoreData}
		hasMore={hasMore}
		loader={<p></p>}
		endMessage={<p>No more games</p>}
	>
	  {games.map((game) => (
		<AchievementItem key={game.titleId} game={game} source={source} />
	  ))}
	</InfiniteScroll>
	</div>
  );
};

export default AchievementsList;
