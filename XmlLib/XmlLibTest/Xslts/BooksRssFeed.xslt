<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:ns="http://library.by/catalog">


  <xsl:output method="xml" indent="yes"/>
  <xsl:template match="ns:catalog">
    <rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom">
      <channel>
        <title>Library RSS Feed Title</title>
        <link>http://library.by/</link>
        <description>Library RSS Feed Description</description>
        <xsl:apply-templates select="ns:book"/>
      </channel>
    </rss>
  </xsl:template>

  <xsl:template match="ns:book">
    <item>
      <title>
        <xsl:value-of select="ns:title"/>
      </title>
      <xsl:if test="ns:isbn != '' and ns:genre = 'Computer'">
        <link>
          <xsl:value-of select="concat('http://my.safaribooksonline.com/', ns:isbn)"/>
        </link>
      </xsl:if>
      <description>
        <xsl:value-of select="ns:description"/>
      </description>
      <pubDate>
        <xsl:value-of select="ns:registration_date"/>
      </pubDate>
    </item>
  </xsl:template>
</xsl:stylesheet>