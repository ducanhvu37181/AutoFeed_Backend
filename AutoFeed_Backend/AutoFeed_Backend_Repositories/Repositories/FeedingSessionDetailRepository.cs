using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories
{
    public class FeedingSessionDetailRepository : GenericRepository<FeedingSessionDetail>
    {
        public FeedingSessionDetailRepository() : base() { }

        public FeedingSessionDetailRepository(AutoFeedDBContext context) : base(context) { }
    }
}
