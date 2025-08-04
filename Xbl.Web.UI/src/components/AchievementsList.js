import React, { useState, useEffect, useRef } from 'react';
import InfiniteScroll from 'react-infinite-scroll-component';
import AchievementItem from './AchievementItem';
import { LAST_UPDATE_KEY, getHeaders, setLastUpdate } from '../lastUpdate';
import searchIcon from '../assets/images/icons8-search-16.png';
import loading from '../assets/images/loading.svg';
import './AchievementsList.css';

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;

const AchievementsList = () => {
	const [games, setGames] = useState([]);
	const [hasMore, setHasMore] = useState(true);
	const [page, setPage] = useState(0);
	const [order, setOrder] = useState('lastPlayed-desc');
	const [source, setSource] = useState('live');
	const [searchVisible, setSearchVisible] = useState(false);
	const [searchQuery, setSearchQuery] = useState('');

	const loadingRef = useRef(null);
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

	async function fetchPage() {
		const o = replaceOrderPostfix(order);
		const headers = await getHeaders(API_BASE_URL);
		const response = await fetch(`${API_BASE_URL}/Titles/${source}?page=${page}&orderBy=${o}&title=${searchQuery}`, { headers: headers });
		return await response.json();
	}

	const fetchFirstPage = async () => {
		const data = await fetchPage();
		setGames(data);
		setPage(1);
		setHasMore(data.length >= 50);
	};

	const fetchMoreData = async () => {
		const data = await fetchPage();
		setGames([...games, ...data]);
		setPage(page + 1);
		if (data.length < 50) {
			setHasMore(false);
		}
	};

	const handleOrderChange = (e) => {
		setOrder(e.target.value);
		setPage(0);
		setHasMore(true);
		setGames([]);
	};

	const handleSourceChange = (e) => {
		setSource(e.target.value);
		setPage(0);
		setHasMore(true);
		setGames([]);
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
		setPage(0);
		setHasMore(true);
		setGames([]);
	};

	const handleSearchBlur = (e) => {
		if (e.relatedTarget !== searchToggleRef.current) {
			setSearchVisible(false);
		}
		e.stopPropagation();
	};

	const update = async (e) => {
		loadingRef.current.classList.remove('d-none');
		const response = await fetch(`${API_BASE_URL}/Titles/update`, { method: "POST" });
		setLastUpdate(response.headers.get(LAST_UPDATE_KEY));
		loadingRef.current.classList.add('d-none');
		setPage(0);
		setHasMore(true);
		setGames([]);
		fetchFirstPage();
	}

	return (
		<div>
			<div className="loading d-none" ref={loadingRef}>
				<img src={loading}></img>
			</div>
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
				<div className="break"></div>
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
				loader={<div className="loading"><img src={loading}></img></div>}
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
