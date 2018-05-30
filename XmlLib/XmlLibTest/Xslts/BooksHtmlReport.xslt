<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="1.0"
                xmlns:ns="http://library.by/catalog"
                xmlns:ext="http://library.by/ext">

  <xsl:output method="html" indent="yes"/>
  <xsl:key name="books-by-genre" match="/ns:catalog/ns:book" use="ns:genre" />
  <xsl:template match="/">
    <html>
      <head>
        <title>
          <xsl:value-of select="ext:GetReportTitle()"/>
        </title>
        <h2>
          <xsl:value-of select="ext:GetReportTitle()"/>
        </h2>
      </head>
      <body>
        <br/>
        <xsl:for-each select="/ns:catalog/ns:book[count(. | key('books-by-genre', ns:genre)[1]) = 1]">
          <xsl:sort select="ns:genre"/>
          <h2>
            <span>
              <xsl:value-of select="ext:GetAndInc('mycounter')"/>.
            </span>
            <xsl:value-of select="ns:genre"/>
          </h2>
          <table style="height: 200px;" width="600">
            <tbody>
              <tr>
                <th>Author</th>
                <th>Name</th>
                <th>Publish Date</th>
                <th>Registration Date</th>
              </tr>
              <xsl:apply-templates select="key('books-by-genre', ns:genre)" />
              <tr>
                <td colspan="3"></td>
                <td>
                  Total: <xsl:value-of select="count(key('books-by-genre', ns:genre))"/>
                </td>
              </tr>
            </tbody>
          </table>
        </xsl:for-each>
        <h2>
          Total genres count: <xsl:value-of select="ext:GetAndInc('mycounter') - 1"/>
        </h2>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="ns:book">
    <tr>
      <td>
        <xsl:value-of select="ns:author"/>
      </td>
      <td>
        <xsl:value-of select="ns:title"/>
      </td>
      <td>
        <xsl:value-of select="ns:publish_date"/>
      </td>
      <td>
        <xsl:value-of select="ns:registration_date"/>
      </td>
    </tr>
  </xsl:template>

</xsl:stylesheet>
