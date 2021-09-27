using System;
// ReSharper disable StringLiteralTypo

namespace Boltmailer_mainserver
{
    public static class Exceptions
    {
        public static string GetProjectNotFoundException(string projectName, string assignedEmployee, string recipient, Exception ex)
        {
            BoltReader.DLog($"\nTried to update non-existent project {projectName} for '{assignedEmployee}'! Notifying email sender ({recipient}).", LogLevel.Warnings);
            
            return $@"
<h1>Suomeksi:</h1>
<h4>Tämä on automaattinen viesti Boltmailer Serveriltä. <b>Ethän yritä vastata tähän viestiin.</b></h4>
</br>
</br>
<h3>Projektia jota yritit päivittää ({projectName}) ei ole olemassa käyttäjälle '{assignedEmployee}':</h3>
<h4>{ex?.Message}</h4>
</br>
<h4>Jos uskot tämän olevan virhe, ota yhteyttä ylläpitäjiin.</h4>
</br></br>
<h1>In English:</h1>
<h4>This is an automated message sent by the Boltmailer Mainserver. <b>Please do not try to reply to this email.</b></h4>
<h3>The project you have tried to update ({projectName}) does not exist for the user '{assignedEmployee}'.</h3>
<h4>If you believe this is an error, please contact the Administrators.</h4>
                                ";
        }
        
        public static string GetUserNotFoundException(string projectName, string assignedEmployee, string recipient, Exception ex)
        {
            BoltReader.DLog($"\nTried to update project {projectName} for non existing user '{assignedEmployee}'! Notifying email sender ({recipient}).", LogLevel.Warnings);
            
            return $@"
<h1>Suomeksi:</h1>
<h4>Tämä on automaattinen viesti Boltmailer Serveriltä. <b>Ethän yritä vastata tähän viestiin.</b></h4>
</br>
</br>
<h3>Käyttäjällä {assignedEmployee} ei ole projektia '{projectName}', jota yrität päivittää. Tarkistathan parametrisi, sekä että päivitettävä projekti on olemassa:</h3>
<h4>{ex?.Message}</h4>
</br>
<h4>Jos uskot tämän olevan virhe, ota yhteyttä ylläpitäjiin.</h4>
</br></br>
<h1>In English:</h1>
<h4>This is an automated message sent by the Boltmailer Mainserver. <b>Please do not try to reply to this email.</b></h4>
<h3>The user {assignedEmployee} does not have the project '{projectName}' that you're trying to update. Please check your parameters.</h3>
<h4>If you believe this is an error, please contact the Administrators.</h4>
                                ";
        }
        
        public static string GetGeneralException(string projectName, string assignedEmployee, string recipient, Exception ex)
        {
            BoltReader.DLog($"\nError while creating/updating project {projectName} for user '{assignedEmployee}'! Notifying email sender ({recipient}).", LogLevel.Warnings);

            return $@"
<h1>Suomeksi:</h1>
<h4>Tämä on automaattinen viesti Boltmailer Serveriltä. <b>Ethän yritä vastata tähän viestiin.</b></h4>
</br>
</br>
<h3>Projektin ({projectName}) lisäämisessä käyttäjälle '{assignedEmployee}' on tapahtunut virhe:</h3>
<h4>{ex?.Message}</h4>
</br></br>
<h1>In English:</h1>
<h4>This is an automated message sent by the Boltmailer Mainserver. <b>Please do not try to reply to this email.</b></h4>
<h3>There's been an error adding a project ({projectName}) for the user '{assignedEmployee}':</h3>
<h4>{ex?.Message}</h4>
                                ";
        }
    }
}