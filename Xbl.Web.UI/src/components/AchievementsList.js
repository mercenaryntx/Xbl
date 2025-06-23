// src/components/AchievementsList.js
import React, { useState, useEffect } from 'react';
import InfiniteScroll from 'react-infinite-scroll-component';
import AchievementItem from './AchievementItem';
import './AchievementsList.css';

const AchievementsList = () => {
  const [games, setGames] = useState([]);
  const [hasMore, setHasMore] = useState(true);
  const [page, setPage] = useState(0);
  const [order, setOrder] = useState('lastPlayed-desc');

  useEffect(() => {
    fetchMoreData();
  }, [order]);
  

	function replaceOrderPostfix(str) {
	  return str.replace(/(.*)-(asc|desc)$/, (match, p1, p2) => {
	    return `${p1}&orderDir=${p2.toUpperCase()}`;
	  });
	}

  const fetchMoreData = async () => {
	const o = replaceOrderPostfix(order);
    const response = await fetch(`https://localhost:7238/Titles?page=${page}&orderBy=${o}`);
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
	//fetchMoreData();
  };
 
  const update = async (e) => {
	const response = await fetch(`https://localhost:7238/Update`);
  }

  return (
	<div>
	<div className="order-selection">
		<button id="update" onClick={update}>Update</button>
		<select id="order" value={order} onChange={handleOrderChange}>
			<option value="name-asc">A-Z &#8595;</option>
			<option value="name-desc">A-Z &#8593;</option>
			<option value="lastPlayed-asc">Last Played &#8595;</option>
			<option value="lastPlayed-desc">Last Played &#8593;</option>
			<option value="progress-asc">Progress &#8595;</option>
			<option value="progress-desc">Progress &#8593;</option>
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
		<AchievementItem key={game.titleId} game={game} />
	  ))}
	</InfiniteScroll>
	</div>
  );
};

export default AchievementsList;
