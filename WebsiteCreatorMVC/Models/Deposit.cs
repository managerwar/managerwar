using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebsiteCreatorMVC.Models
{
    public class Deposit
    {
        [Key]
        public long ID { get; set; }
        public string User_Id { get; set; }

        [ForeignKey("User_Id")]
        public ApplicationUser User { get; set; }

        public long RoundID_ID { get; set; }

        [ForeignKey("RoundID_ID")]
        public Round RoundID { get; set; }


        public bool Red { get; set; }

        [Column(TypeName="money")]
        public decimal Amount { get; set; }
        
        [Column(TypeName="money")]
        public decimal Fee { get; set; }

        [Column(TypeName="money")]
        public decimal FeePercentage { get; set; }

        // 1 = confirmed
        // 2 = Confirmed too late
        // 3 = unconfirmed
        public byte Status { get; set; }

        // 1 == AsMoney
        // 2 == Bitcoin
        // 3 == Litecoin
        public byte PaymentType { get; set; }

        public long BatchNo { get; set; }

    } // Deposit

    public class TestDeposit
    {
        public decimal Amount { get; set; }

        public bool Red { get; set; }

    }
}