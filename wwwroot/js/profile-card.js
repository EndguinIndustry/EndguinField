function editProfile(userId) {
    window.location.href = `/Profile/Edit/${userId}`;
}

function viewFriendRequests(userId) {
    window.location.href = `/Profile/FriendRequests/${userId}`;
}

//Loading animation
document.addEventListener('DOMContentLoaded', function() {
    const profileCard = document.querySelector('.profile-card');
    
    if (profileCard) {
        profileCard.style.opacity = '0';
        profileCard.style.transform = 'translateY(20px)';
        
        setTimeout(() => {
            profileCard.style.transition = 'all 0.5s ease';
            profileCard.style.opacity = '1';
            profileCard.style.transform = 'translateY(0)';
        }, 100);
        
        const statItems = document.querySelectorAll('.stat-item');
        statItems.forEach((item, index) => {
            item.style.cursor = 'pointer';
            item.addEventListener('click', function() {
                const label = this.querySelector('.stat-label').textContent.trim();
                if (label.includes('Friends')) {
                    window.location.href = '/Profile/Friends';
                } else if (label.includes('Enrollments')) {
                    window.location.href = '/Profile/Enrollments';
                } else if (label.includes('Pending')) {
                    window.location.href = '/Profile/FriendRequests';
                }
            });
        });
    }
});