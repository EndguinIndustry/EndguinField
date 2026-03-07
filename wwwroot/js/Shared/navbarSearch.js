document.addEventListener('DOMContentLoaded', () => {
    const input = document.getElementById('navbar-search-input');
    const resultBox = document.getElementById('navbar-search-results');

    if (!input || !resultBox) {
        return;
    }

    const state = {
        loaded: false,
        loading: false,
        events: [],
        users: []
    };

    const normalize = (value) => (value || '').toString().trim().toLowerCase();
    const escapeHtml = (value) =>
        (value || '')
            .toString()
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    const formatEventDate = (value) => {
        if (!value) {
            return '';
        }

        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return '';
        }

        return date.toLocaleString('en-GB', {
            day: '2-digit',
            month: 'short',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const hideResults = () => {
        resultBox.hidden = true;
        resultBox.innerHTML = '';
    };

    const renderWithCloseButton = (content) => `
        <div class="search-results-top">
            <button type="button" class="search-results-close" aria-label="Close search results">
                <img src="/assets/icons/decline.svg" alt="Close" class="search-results-close-icon">
            </button>
        </div>
        ${content}
    `;

    const renderSection = (title, items, itemType) => {
        if (!items.length) {
            return '';
        }

        const rows = items
            .map(item => {
                const isEvent = itemType === 'event';
                if (isEvent) {
                    const eventDate = formatEventDate(item.startTime);
                    const participantCount = Number(item.participantCount ?? 0);
                    const maxParticipants = Number(item.maxParticipants ?? 0);

                    return `
                            <a class="EventLink search-event-link" href="${escapeHtml(item.url)}">
                                <div class=" search-event-card">
                                    <img src="${escapeHtml(item.image || '/asset/profile.svg')}" alt="EventBanner">
                                    <div class="CardInfo">
                                        <div class="CardDetail">
                                            <h1>${escapeHtml(item.name)}</h1>
                                            <p>Date: ${escapeHtml(eventDate)} <br> Location: ${escapeHtml(item.location || '-')}</p>
                                        </div>
                                        <div class="Participant">
                                            <h1>${participantCount}/${maxParticipants}</h1>
                                        </div>
                                    </div>
                                </div>
                            </a>
                        `;
                }

                return `
                    <a class="EventLink search-event-link" href="${escapeHtml(item.url)}">
                        <div class=" search-event-card">
                            <img src="${escapeHtml(item.image || '/asset/profile.svg')}" alt="${escapeHtml(item.name)}">
                            <div class="CardInfo">
                                <div class="CardDetail">
                                    <h1>${escapeHtml(item.name)}</h1>
                                    <p>User profile</p>
                                </div>

                            </div>
                        </div>
                    </a>
                `;
            })
            .join('');

        return `
            <div class="search-result-section">
                <div class="search-result-header">${title}</div>
                ${rows}
            </div>
        `;
    };

    const renderResults = (query) => {
        const keyword = normalize(query);

        if (!keyword) {
            hideResults();
            return;
        }

        const matchedEvents = state.events.filter(item => normalize(item.name).includes(keyword));
        const matchedUsers = state.users.filter(item => normalize(item.name).includes(keyword));

        if (!matchedEvents.length && !matchedUsers.length) {
            resultBox.innerHTML = renderWithCloseButton('<div class="search-empty">No results found</div>');
            resultBox.hidden = false;
            return;
        }

        resultBox.innerHTML = renderWithCloseButton(
            `${renderSection('Events', matchedEvents.slice(0, 6), 'event')}${renderSection('Users', matchedUsers.slice(0, 6), 'user')}`
        );
        resultBox.hidden = false;
    };

    const loadSearchData = async ({ force = false } = {}) => {
        if (state.loading) {
            return;
        }

        if (state.loaded && !force) {
            return;
        }

        state.loading = true;

        try {
            const response = await fetch('/API/Search/NavbarData', {
                method: 'GET',
                credentials: 'same-origin'
            });

            if (!response.ok) {
                throw new Error('Failed to load search data');
            }

            const payload = await response.json();
            state.events = Array.isArray(payload.events) ? payload.events : [];
            state.users = Array.isArray(payload.users) ? payload.users : [];
            state.loaded = true;
        } catch {
            state.loaded = false;
        } finally {
            state.loading = false;
        }
    };

    input.addEventListener('focus', async () => {
        await loadSearchData({ force: true });
        if (input.value.trim()) {
            renderResults(input.value);
        }
    });

    input.addEventListener('input', async (event) => {
        await loadSearchData();
        renderResults(event.target.value);
    });

    document.addEventListener('click', (event) => {
        if (!resultBox.contains(event.target) && event.target !== input) {
            hideResults();
        }
    });

    resultBox.addEventListener('click', (event) => {
        if (event.target.closest('.search-results-close')) {
            hideResults();
            event.stopPropagation();
        }
    });

    input.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') {
            hideResults();
            input.blur();
        }
    });
});
