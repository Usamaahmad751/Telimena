﻿namespace Telimena.Client
{
    using System;

    public class RegistrationResponse : TelimenaResponseBase
    {

        public int UserId { get; set; }
        public int ProgramId { get; set; }
        public int Count { get; set; }
    }
}