using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DvMod.Challenges
{
    public class Status
    {
        public static string fileName = @"ChallengeStatus.xml";
        public static byte[] fileBuffer = new byte[20480];
        public static int bytesRead = 0;
        public static byte[] preAllJobs = new byte[20480];
        public static int challengesStart = 0;
        public static byte[] postAllJobs = new byte[20480];
        public static int challengesEnd = 0;
        public static AllJob[] allJobs = new AllJob[15];


        public static string save(AllJob curJob)
        {
            string retVal = "";

            retVal = readStatus();

            if(!retVal.Equals(""))
            {
                retVal = createNewFile();
                if (retVal.Equals(""))
                {
                    retVal = readStatus();
                }
            }
            if (!retVal.Equals("")) return retVal;

            retVal = parseFileBuffer();

            if(retVal.Equals(""))
            {
                for(int i = 0; i < allJobs.Length; i++)
                {
                    if(allJobs[i].stationId == curJob.stationId)
                    {
                        allJobs[i].status = curJob.status;
                        allJobs[i].message = curJob.message;
                        allJobs[i].jobs = curJob.jobs;
                        break;  
                    }
                }

                writeStatusFile();
            }

            return retVal;

        }

        public static AllJob getJobStatus(string stationId)
        {
            AllJob retJob = new AllJob();

            string retVal = "";

            retVal = readStatus();

            if (!retVal.Equals(""))
            {
                retVal = createNewFile();
                if (retVal.Equals(""))
                {
                    retVal = readStatus();
                }
            }
            if (!retVal.Equals("")) return retJob;

            retVal = parseFileBuffer();

            if (retVal.Equals(""))
            {
                for (int i = 0; i < allJobs.Length; i++)
                {
                    if (allJobs[i].stationId == stationId)
                    {
                        retJob = allJobs[i];
                        break;
                    }
                }
            }
            return retJob;
        }

        public static string parseFileBuffer()
        {
            string retVal = "";

            try
            {

                string fileBufStr = Encoding.UTF8.GetString(fileBuffer, 0, bytesRead);

                challengesStart = fileBufStr.IndexOf("<AllJobs>\n");
                challengesEnd = fileBufStr.IndexOf("</AllJobs>\n");

                if (challengesStart == -1 || challengesEnd == -1)
                {
                    retVal = "AllJobs section not found; " + challengesStart + " " + challengesEnd;
                }
                else
                {
                    preAllJobs = Encoding.UTF8.GetBytes(fileBufStr.Substring(0, challengesStart));
                    postAllJobs = Encoding.UTF8.GetBytes(fileBufStr.Substring(challengesEnd + 12));
                }

                for (int i = 0; i < AllJob.StationIds.Length; i++)
                {
                    int stationStart = fileBufStr.IndexOf("<Id>" + AllJob.StationIds[i] + "</Id>");
                    if (stationStart >= 0 && stationStart < challengesEnd)
                    {
                        int statusStart = fileBufStr.Substring(stationStart).IndexOf("<Status>");
                        int statusEnd = fileBufStr.Substring(stationStart).IndexOf("</Status>");
                        int messageStart = fileBufStr.Substring(stationStart).IndexOf("<Message>");
                        int messageEnd = fileBufStr.Substring(stationStart).IndexOf("</Message>");
                        int jobsStart = fileBufStr.Substring(stationStart).IndexOf("<Jobs>");
                        int jobsEnd = fileBufStr.Substring(stationStart).IndexOf("</Jobs>");
                        if (statusStart < 0 || statusStart > challengesEnd || messageStart < 0 || messageStart > challengesEnd ||
                            statusEnd < 0 || statusEnd > challengesEnd || messageEnd < 0 || messageEnd > challengesEnd ||
                            jobsStart < 0 || jobsStart > challengesEnd || jobsEnd < 0 || jobsEnd > challengesEnd)
                        {
                            retVal = "could not find status or message for " + AllJob.StationIds[i] + " " + statusStart + " " + statusEnd + " " + messageStart + " " + messageEnd + " " + jobsStart + " " + jobsEnd;
                            break;
                        }
                        else
                        {
                            statusStart += "<Status>".Length;
                            messageStart += "<Message>".Length;
                            jobsStart += "<Jobs>".Length;
                            AllJob addJob = new AllJob();
                            addJob.stationId = AllJob.StationIds[i];
                            addJob.status = fileBufStr.Substring(statusStart + stationStart, statusEnd - statusStart);
                            addJob.message = fileBufStr.Substring(messageStart + stationStart, messageEnd - messageStart);
                            addJob.jobs = fileBufStr.Substring(jobsStart + stationStart, jobsEnd - jobsStart);
                            allJobs[i] = addJob;
                        }
                    }
                    else
                    {
                        retVal = "Could not find station start for " + AllJob.StationIds[i] + " " + stationStart;
                    }
                }
            }
            catch (Exception ex)
            {
                retVal = ex.Message;
            }

            return retVal;
        }

        public static string readStatus()
        {
            string retVal = "";
            try
            {
                using FileStream fs = File.OpenRead(fileName);

                int c;

                c = fs.Read(fileBuffer, 0, fileBuffer.Length);

                if (c == 0 || c == fileBuffer.Length)
                {
                    retVal = "invalid file, length = " + c;
                }
                bytesRead = c;
                fs.Close();

            }
            catch (Exception ex)
            {
                retVal = ex.Message;
            }

            return retVal;
        }
        private static string createNewFile()
        {
            string retVal = "";
            for (int i = 0; i < allJobs.Length; i++)
            {
                AllJob job = new AllJob();
                job.stationId = AllJob.StationIds[i];
                job.status = "Not Started";
                job.message = "";
                job.jobs = "";
                allJobs[i] = job;
            }

            retVal = writeStatusFile();

            return retVal;
        }

        private static string writeStatusFile()
        {
            string retVal = "";

            FileStream fs = File.OpenWrite(fileName);

            var data = "<? xml version = \"1.0\" encoding = \"utf - 8\" ?>\n";
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            fs.Write(bytes, 0, bytes.Length);

            data = "   <ChallengeStatus xmlns:xsd = \"http://www.w3.org/2001/XMLSchema\" xmlns: xsi = \"http://www.w3.org/2001/XMLSchema-instance\">\n";
            bytes = Encoding.UTF8.GetBytes(data);
            fs.Write(bytes, 0, bytes.Length);

            data = "      <AllJobs>\n";
            bytes = Encoding.UTF8.GetBytes(data);
            fs.Write(bytes, 0, bytes.Length);

            for(int i = 0; i < allJobs.Length; i++)
            {
                writeStation(fs,allJobs[i]);
            }

            data = "      </AllJobs>\n";
            bytes = Encoding.UTF8.GetBytes(data);
            fs.Write(bytes, 0, bytes.Length);

            data = "   </ChallengeStatus>\n";
            bytes = Encoding.UTF8.GetBytes(data);
            fs.Write(bytes, 0, bytes.Length);

            fs.Flush();
            fs.Close();

            return retVal;
        }

        public static string writeStation(FileStream fs, AllJob job)
        {
            string retVal = "";

            var data = "         <Station>\n";
            byte[]  bytes = Encoding.UTF8.GetBytes(data);
            fs.Write(bytes, 0, bytes.Length);

            data = "            <Id>" + job.stationId + "</Id>\n";
            bytes = Encoding.UTF8.GetBytes(data);
            fs.Write(bytes, 0, bytes.Length);

            data = "            <Status>"+job.status+"</Status>\n";
            bytes = Encoding.UTF8.GetBytes(data);
            fs.Write(bytes, 0, bytes.Length);

            data = "            <Message>" + job.message + "</Message>\n";
            bytes = Encoding.UTF8.GetBytes(data);
            fs.Write(bytes, 0, bytes.Length);

            data = "            <Jobs>" + job.jobs + "</Jobs>\n";
            bytes = Encoding.UTF8.GetBytes(data);
            fs.Write(bytes, 0, bytes.Length);

            data = "         </Station>\n";
            bytes = Encoding.UTF8.GetBytes(data);
            fs.Write(bytes, 0, bytes.Length);

            return retVal;

        }
    }
}
